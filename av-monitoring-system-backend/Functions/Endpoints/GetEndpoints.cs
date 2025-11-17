using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Endpoints;

public class GetEndpoints
{
    private readonly EndpointRepository _repo;

    public GetEndpoints(EndpointRepository repo)
    {
        _repo = repo;
    }

    [Function("GetEndpoints")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestData req)
    {
        var endpoints = await _repo.GetAllAsync();

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(endpoints);
        return res;
    }
}
