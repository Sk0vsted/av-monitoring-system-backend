using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Services;

public class MonitoringAnalyticsService
{
    private readonly MonitoringLogRepository _logRepo;
    private readonly IncidentRepository _incidentRepo;

    public MonitoringAnalyticsService(
        MonitoringLogRepository logRepo,
        IncidentRepository incidentRepo)
    {
        _logRepo = logRepo;
        _incidentRepo = incidentRepo;
    }

    public async Task<DailyAnalyticsEntity> BuildGlobalDailyAnalyticsAsync()
    {
        var logs = await _logRepo.GetAllAsync();
        var incidents = await _incidentRepo.GetAllAsync();

        var total = logs.Count;
        var successes = logs.Count(x => x.StatusCode < 400);

        var entity = new DailyAnalyticsEntity
        {
            PartitionKey = "GLOBAL",
            RowKey = DateTime.UtcNow.Date.ToString("yyyy-MM-dd"),
            Date = DateTime.UtcNow.Date,

            AverageResponseTimeMs = logs.Any()
                ? logs.Average(x => x.ResponseTimeMs)
                : 0,

            Errors4xx = logs.Count(x => x.StatusCode >= 400 && x.StatusCode < 500),
            Errors5xx = logs.Count(x => x.StatusCode >= 500),
            Timeouts = logs.Count(x => x.StatusCode == 0 || x.StatusCode == 408),

            CriticalIncidents = incidents.Count(x => x.Severity == "Critical"),
            WarningIncidents = incidents.Count(x => x.Severity == "Warning"),
            InfoIncidents = incidents.Count(x => x.Severity == "Info"),

            UptimePercentage = total == 0
                ? 0
                : (double)successes / total * 100,
        };

        return entity;
    }
}
