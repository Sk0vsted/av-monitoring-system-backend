using System.Net;
using System.Text.Json;
using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Models.Endpoints;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions.Endpoints;

public class CreateEndpoint
{
    private readonly EndpointRepository _repo;

    public CreateEndpoint(EndpointRepository repo)
    {
        _repo = repo;
    }

    [Function("CreateEndpoint")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req)
    {
        var json = await new StreamReader(req.Body).ReadToEndAsync();

        var body = JsonSerializer.Deserialize<CreateEndpointRequest>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        if (body == null || string.IsNullOrWhiteSpace(body.Url))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid request body.");
            return bad;
        }

        var entity = EndpointMapper.ToEntity(body);
        entity.PartitionKey = "Endpoint";
        entity.RowKey = Guid.NewGuid().ToString();

        await _repo.AddAsync(entity);

        var res = req.CreateResponse(HttpStatusCode.Created);
        await res.WriteAsJsonAsync(entity);
        return res;
    }
}
