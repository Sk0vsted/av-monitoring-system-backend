namespace AVMonitoring.Functions.Models;

public class MonitoredEndpoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; } = 30;
    public DateTime LastPingUtc { get; set; } = DateTime.MinValue;
}
