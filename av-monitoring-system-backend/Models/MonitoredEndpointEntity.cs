using Azure;
using Azure.Data.Tables;

namespace AVMonitoring.Functions.Models;

public class MonitoredEndpointEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Endpoints";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; }
    public DateTime LastPingUtc { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
