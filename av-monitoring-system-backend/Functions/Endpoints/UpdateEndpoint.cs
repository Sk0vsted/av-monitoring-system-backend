using System.Net;
using System.Text.Json;
using AVMonitoring.Functions.Models.Endpoints;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Endpoints;

public class UpdateEndpoint
{
    private readonly EndpointRepository _repo;

    public UpdateEndpoint(EndpointRepository repo)
    {
        _repo = repo;
    }

    [Function("UpdateEndpoint")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put",
            Route = "endpoints/{id}")]
        HttpRequestData req,
        string id)
    {
        var existing = await _repo.GetByIdAsync(id);

        if (existing == null)
        {
            var nf = req.CreateResponse(HttpStatusCode.NotFound);
            await nf.WriteStringAsync("Endpoint not found.");
            return nf;
        }

        var json = await new StreamReader(req.Body).ReadToEndAsync();
        var body = JsonSerializer.Deserialize<UpdateEndpointRequest>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (body == null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid request body.");
            return bad;
        }

        if (!string.IsNullOrWhiteSpace(body.Name))
            existing.Name = body.Name;

        if (!string.IsNullOrWhiteSpace(body.Url))
            existing.Url = body.Url;

        if (!string.IsNullOrWhiteSpace(body.Method))
            existing.Method = body.Method;

        if (body.IntervalSeconds.HasValue)
            existing.IntervalSeconds = body.IntervalSeconds.Value;

        if (!string.IsNullOrWhiteSpace(body.HeadersJson))
            existing.HeadersJson = body.HeadersJson;

        if (body.Body != null)
            existing.BodyJson = body.Body;

        if (body.AuthHeader != null)
            existing.AuthHeader = body.AuthHeader;

        await _repo.UpsertAsync(existing);

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(existing);
        return ok;
    }
}
