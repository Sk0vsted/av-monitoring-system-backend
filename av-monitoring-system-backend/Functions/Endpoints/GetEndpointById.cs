using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions;

public class GetEndpointById
{
    private readonly EndpointRepository _repo;

    public GetEndpointById(EndpointRepository repo)
    {
        _repo = repo;
    }

    [Function("GetEndpointById")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "endpoints/{id}")]
        HttpRequestData req,
        string id)
    {
        var endpoint = await _repo.GetByIdAsync(id);

        if (endpoint == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Endpoint not found.");
            return notFound;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(endpoint);
        return ok;
    }
}
