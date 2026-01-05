using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Logs
{
    public class GetLogs
    {
        private readonly MonitoringLogRepository _logs;

        public GetLogs(MonitoringLogRepository logs)
        {
            _logs = logs;
        }

        [Function("GetLogs")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestData req)
        {
            var data = await _logs.GetAllAsync();

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(data);
            return res;
        }
    }
}
