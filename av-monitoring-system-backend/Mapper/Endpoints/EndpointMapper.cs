using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Models.Endpoints;
using System.Text.Json;

public static class EndpointMapper
{
    public static MonitoredEndpointEntity ToEntity(CreateEndpointRequest req)
    {
        return new MonitoredEndpointEntity
        {
            Name = req.Name,
            Url = req.Url,
            Method = req.Method,
            IntervalSeconds = req.IntervalSeconds,
            CreatedUtc = DateTime.UtcNow,
            HeadersJson = req.Headers != null ? JsonSerializer.Serialize(req.Headers) : null,
            BodyJson = req.Body,
            AuthHeader = req.AuthHeader
        };
    }
}
