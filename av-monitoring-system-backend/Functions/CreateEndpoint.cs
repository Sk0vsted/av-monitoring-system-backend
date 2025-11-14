using System.Net;
using System.Text.Json;
using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions;

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

        var entity = new MonitoredEndpointEntity
        {
            Url = body.Url,
            IntervalSeconds = body.IntervalSeconds,
            LastPingUtc = DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc)
        };

        await _repo.AddAsync(entity);

        var res = req.CreateResponse(HttpStatusCode.Created);
        await res.WriteAsJsonAsync(entity);
        return res;
    }
}
