using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Logs;

public class GetLogsByEndpointId
{
    private readonly MonitoringLogRepository _logs;

    public GetLogsByEndpointId(MonitoringLogRepository logs)
    {
        _logs = logs;
    }

    [Function("GetLogsByEndpointId")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetLogsByEndpointId/{id}")]
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
