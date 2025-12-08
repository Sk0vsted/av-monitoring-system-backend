namespace AVMonitoring.Functions.Models.Endpoints;
public class UpdateEndpointRequest
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Method { get; set; }
    public int? IntervalSeconds { get; set; }
    public string? HeadersJson { get; set; }
    public string? Body { get; set; }
    public string? AuthHeader { get; set; }
}
