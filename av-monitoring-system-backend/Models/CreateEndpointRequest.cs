namespace AVMonitoring.Functions.Models;

public class CreateEndpointRequest
{
    public string Url { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; }
}
