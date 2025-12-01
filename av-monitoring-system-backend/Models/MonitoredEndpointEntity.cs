using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace AVMonitoring.Functions.Models;

public class MonitoredEndpointEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Endpoint";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public int IntervalSeconds { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastPingUtc { get; set; } = DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc);
    public string? HeadersJson { get; set; }
    public string? BodyJson { get; set; }
    public string? AuthHeader { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public bool IsInCooldown { get; set; }
    public DateTime? CooldownUntilUtc { get; set; }
    public int ConsecutiveCriticalErrors { get; set; }

}

