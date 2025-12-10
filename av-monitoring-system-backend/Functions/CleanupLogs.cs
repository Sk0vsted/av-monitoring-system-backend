using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;

public class CleanupLogs
{
    private readonly MonitoringLogRepository _logRepo;
    private readonly MonitoringAnalyticsService _analyticsService;
    private readonly MonitoringAnalyticsRepository _analyticsRepo;
    private readonly EndpointRepository _endpointRepo;

    public CleanupLogs(
        MonitoringLogRepository logRepo,
        MonitoringAnalyticsService analyticsService,
        MonitoringAnalyticsRepository analyticsRepo,
        EndpointRepository endpointRepo)
    {
        _logRepo = logRepo;
        _analyticsService = analyticsService;
        _analyticsRepo = analyticsRepo;
        _endpointRepo = endpointRepo;
    }

    [Function("CleanupLogs")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timer)
    {
        var endpoints = await _endpointRepo.GetAllAsync();

        foreach (var endpoint in endpoints)
        {
            var daily = await _analyticsService.BuildGlobalDailyAnalyticsAsync();

            await _analyticsRepo.SaveDailyAnalyticsAsync(daily);
        }
        await _logRepo.DeleteAllAsync();
    }
}
