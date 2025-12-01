using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions;

public class DeleteEndpoint
{
    private readonly EndpointRepository _repo;

    public DeleteEndpoint(EndpointRepository repo)
    {
        _repo = repo;
    }

    [Function("DeleteEndpoint")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
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

        await _repo.DeleteAsync(endpoint.PartitionKey, endpoint.RowKey);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
}
