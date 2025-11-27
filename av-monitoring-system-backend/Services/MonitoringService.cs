using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Services
{
    public class MonitoringService
    {
        private readonly EndpointRepository _endpointRepo;
        private readonly MonitoringLogRepository _logRepo;
        private readonly IncidentRepository _incidentRepo;
        private readonly IncidentEngine _incidentEngine;
        private readonly IHttpPingService _ping;

        public MonitoringService(
            EndpointRepository endpointRepo,
            MonitoringLogRepository logRepo,
            IncidentRepository incidentRepo,
            IncidentEngine incidentEngine,
            IHttpPingService ping)
        {
            _endpointRepo = endpointRepo;
            _logRepo = logRepo;
            _incidentRepo = incidentRepo;
            _incidentEngine = incidentEngine;
            _ping = ping;
        }

        public async Task CheckAsync()
        {
            var endpoints = await _endpointRepo.GetAllAsync();
            var now = DateTime.UtcNow;

            foreach (var ep in endpoints)
            {
                // Skip hvis endpoint er i cooldown
                if (ep.IsInCooldown && ep.CooldownUntilUtc > now)
                    continue;

                // Første ping: LastPingUtc = 1970 → ping direkte
                var secondsSinceLast = (now - ep.LastPingUtc).TotalSeconds;

                if (secondsSinceLast < ep.IntervalSeconds)
                    continue;

                // Ping
                var result = await _ping.PingAsync(ep);

                // Opdater
                ep.LastPingUtc = now;

                // === Critical error handling ===
                if (result.IsError || result.StatusCode >= 500)
                {
                    ep.ConsecutiveCriticalErrors++;

                    if (ep.ConsecutiveCriticalErrors >= 3)
                    {
                        ep.IsInCooldown = true;
                        ep.CooldownUntilUtc = now.AddMinutes(5);
                    }
                }
                else
                {
                    // Reset når ping lykkes
                    ep.ConsecutiveCriticalErrors = 0;
                    ep.IsInCooldown = false;
                }

                await _endpointRepo.UpsertAsync(ep);

                // Log
                var log = new MonitoringLogEntity
                {
                    PartitionKey = ep.RowKey,
                    RowKey = DateTime.UtcNow.Ticks.ToString(),
                    Url = ep.Url,
                    Name = ep.Name,
                    Method = ep.Method,
                    StatusCode = result.StatusCode,
                    ResponseTimeMs = result.ResponseTimeMs,
                    IsError = result.IsError,
                    PingedAtUtc = now
                };

                await _logRepo.AddAsync(log);

                // Incident analyse
                var recentLogs = await _logRepo.GetLastNAsync(ep.RowKey, 25);

                var incidents = _incidentEngine.Analyze(recentLogs);

                foreach (var inc in incidents)
                {
                    var entity = new IncidentEntity
                    {
                        PartitionKey = ep.RowKey,
                        RowKey = Guid.NewGuid().ToString(),
                        EndpointName = ep.Name,
                        IncidentType = inc.IncidentType,
                        Severity = inc.Severity,
                        Message = inc.Message,
                        ValueNow = inc.ValueNow,
                        ValueBaseline = inc.ValueBaseline,
                        CreatedUtc = inc.CreatedUtc
                    };

                    await _incidentRepo.SaveAsync(entity);
                }
            }
        }
    }
}
