using System;
using System.Threading.Tasks;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions;

public class DeleteIncidents
{
    private readonly IncidentRepository _repo;

    public DeleteIncidents(IncidentRepository repo)
    {
        _repo = repo;
    }

    [Function("DeleteIncidents")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "incidents")] HttpRequestData req)
    {
        await _repo.DeleteAllAsync();

        var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await res.WriteStringAsync("All logs have been deleted.");

        return res;
    }
}
