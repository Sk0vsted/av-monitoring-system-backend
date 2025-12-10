using Azure;
using Azure.Data.Tables;

namespace AVMonitoring.Functions.Models;

public class DailyAnalyticsEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTime Date { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public int Errors4xx { get; set; }
    public int Errors5xx { get; set; }
    public int Timeouts { get; set; }
    public int CriticalIncidents { get; set; }
    public int WarningIncidents { get; set; }
    public int InfoIncidents { get; set; }
    public double UptimePercentage { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
