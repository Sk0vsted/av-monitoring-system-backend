using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Options;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace AVMonitoring.Functions.Services;

public class MonitoringService
{
    private readonly TableClient _logs;
    private readonly IHttpPingService _ping;
    private readonly EndpointRepository _repo;

    public MonitoringService(
        IOptions<StorageOptions> opts,
        TableServiceClient svc,
        IHttpPingService ping,
        EndpointRepository repo)
    {
        _logs = svc.GetTableClient(opts.Value.MonitoringTable);
        _logs.CreateIfNotExists();

        _ping = ping;
        _repo = repo;
    }

    public async Task CheckAsync()
    {
        var endpoints = await _repo.GetAllAsync();

        foreach (var ep in endpoints)
        {
            var secondsSinceLast = (DateTime.UtcNow - ep.LastPingUtc).TotalSeconds;

            if (secondsSinceLast >= ep.IntervalSeconds)
            {
                // Ping endpoint
                var result = await _ping.PingAsync(ep.Url);

                // Update endpoint state
                ep.LastPingUtc = DateTime.UtcNow;
                await _repo.UpsertAsync(ep);

                // Write log entry
                await _logs.AddEntityAsync(new MonitoringLogEntity
                {
                    PartitionKey = ep.RowKey,
                    RowKey = DateTime.UtcNow.Ticks.ToString(),
                    Url = ep.Url,
                    StatusCode = result.StatusCode,
                    ResponseTimeMs = result.ResponseTimeMs,
                    IsError = result.IsError,
                    ErrorMessage = result.ErrorMessage,
                    PingedAtUtc = DateTime.UtcNow
                });
            }
        }
    }
}
