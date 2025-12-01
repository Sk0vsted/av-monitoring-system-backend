using AVMonitoring.Functions.Models;

namespace AVMonitoring.Functions.Services;

public interface IHttpPingService
{
    Task<PingResult> PingAsync(MonitoredEndpointEntity endpoint);
}
