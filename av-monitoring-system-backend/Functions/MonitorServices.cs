using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Timer;


namespace AVMonitoring.Functions.Functions;

public class MonitorServices
{
    private readonly MonitoringService _monitor;

    public MonitorServices(MonitoringService monitor)
    {
        _monitor = monitor;
    }

    [Function("MonitorServices")]
    public async Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo timer)
    {
        await _monitor.CheckAsync();
    }
}
