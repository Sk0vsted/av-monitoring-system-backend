namespace AVMonitoring.Functions.Models.Endpoints;

public class CreateEndpointRequest
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public int IntervalSeconds { get; set; }
    public string? HeadersJson { get; set; }
    public string? Body { get; set; }
    public string? AuthHeader { get; set; }
}
