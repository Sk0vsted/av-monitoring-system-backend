namespace AVMonitoring.Functions.Models;
public class PingResult
{
    public string Url { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public int StatusCode { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}
