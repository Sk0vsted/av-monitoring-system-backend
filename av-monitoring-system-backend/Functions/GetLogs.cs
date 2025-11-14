using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions;

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
        var endpointId = req.Query["endpointId"];

        if (string.IsNullOrWhiteSpace(endpointId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing endpointId");
            return bad;
        }

        var data = await _logs.GetByEndpointAsync(endpointId);

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(data);
        return res;
    }
}
