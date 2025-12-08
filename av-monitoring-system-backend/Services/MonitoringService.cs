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
                if (ep.IsInCooldown && ep.CooldownUntilUtc > now)
                    continue;

                var secondsSinceLast = (now - ep.LastPingUtc).TotalSeconds;

                if (secondsSinceLast < ep.IntervalSeconds)
                    continue;

                PingResult result;
                try
                {
                    result = await _ping.PingAsync(ep);
                }
                catch (Exception ex)
                {
                    result = new PingResult
                    {
                        StatusCode = 0,
                        ResponseTimeMs = 0,
                        IsError = true
                    };
                }

                ep.LastPingUtc = now;

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
                    ep.ConsecutiveCriticalErrors = 0;
                    ep.IsInCooldown = false;
                }

                await _endpointRepo.UpsertAsync(ep);

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

                var recentLogs = await _logRepo.GetLastNAsync(ep.RowKey, 25);

                var incidents = await _incidentEngine.AnalyzeAsync(ep.RowKey, recentLogs);

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
