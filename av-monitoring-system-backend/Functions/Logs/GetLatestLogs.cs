using System.Linq;
using System.Net;
using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Logs
{
    public class GetLatestLogs
    {
        private readonly MonitoringLogRepository _repo;

        public GetLatestLogs(MonitoringLogRepository repo)
        {
            _repo = repo;
        }

        [Function("GetLatestLogs")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestData req)
        {
            var logs = await _repo.GetAllAsync();

            var result = logs
                .GroupBy(l => l.PartitionKey)
                .Select(g =>
                {
                    var sorted = g.OrderByDescending(x => x.PingedAtUtc).ToList();
                    return new
                    {
                        Name = sorted[0].Name,
                        Url = sorted[0].Url,
                        Method = sorted[0].Method,
                        Current = sorted[0].ResponseTimeMs,
                        Previous = sorted.Count > 1 ? sorted[1].ResponseTimeMs : sorted[0].ResponseTimeMs,
                        StatusCode = sorted[0].StatusCode,
                        PingedAtUtc = sorted[0].PingedAtUtc
                    };
                })
                .ToList();

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(result);
            return res;
        }
    }
}
