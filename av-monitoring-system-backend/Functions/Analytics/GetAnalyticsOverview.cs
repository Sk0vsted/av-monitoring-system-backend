using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Endpoints;

public class GetAnalyticsOverview
{
    private readonly MonitoringAnalyticsRepository _repo;

    public GetAnalyticsOverview(MonitoringAnalyticsRepository repo)
    {
        _repo = repo;
    }

    [Function("GetAnalyticsOverview")]
    public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
    HttpRequestData req)
    {
        var data = await _repo.GetAllAsync();

        var dto = new AnalyticsOverviewDto
        {
            Daily = data
        };

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(dto);
        return res;
    }

}
