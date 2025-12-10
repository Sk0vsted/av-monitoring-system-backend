using System;
using System.Threading.Tasks;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions
{
    public class CleanupLogsManual
    {
        private readonly MonitoringLogRepository _logRepo;
        private readonly MonitoringAnalyticsService _analyticsService;
        private readonly MonitoringAnalyticsRepository _analyticsRepo;
        private readonly EndpointRepository _endpointRepo;

        public CleanupLogsManual(
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

        [Function("CleanupLogsManual")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var endpoints = await _endpointRepo.GetAllAsync();

            var daily = await _analyticsService.BuildGlobalDailyAnalyticsAsync();
            await _analyticsRepo.SaveDailyAnalyticsAsync(daily);

            await _logRepo.DeleteAllAsync();

            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteStringAsync("All logs have been deleted.");

            return res;
        }

    }
}
