using System;
using System.Threading.Tasks;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions
{
    public class CleanupLogsManual
    {
        private readonly MonitoringLogRepository _repo;

        public CleanupLogsManual(MonitoringLogRepository repo)
        {
            _repo = repo;
        }

        [Function("CleanupLogsManual")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            await _repo.DeleteAllAsync();

            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteStringAsync("All logs have been deleted.");

            return res;
        }

    }
}
