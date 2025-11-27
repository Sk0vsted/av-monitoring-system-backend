using Azure;
using Azure.Data.Tables;

namespace AVMonitoring.Functions.Models;

public class MonitoringLogEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = DateTime.UtcNow.Ticks.ToString();
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool IsError { get; set; }
    public DateTime PingedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Method { get; set; } = "GET";
}
