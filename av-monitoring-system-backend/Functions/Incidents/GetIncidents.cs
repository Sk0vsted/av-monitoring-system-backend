using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Functions.Incidents
{
    public class GetIncidents
    {
        private readonly IncidentRepository _repo;

        public GetIncidents(IncidentRepository repo)
        {
            _repo = repo;
        }

        [Function("GetIncidents")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
            HttpRequestData req)
        {
            var incidents = await _repo.GetAllAsync();
            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteAsJsonAsync(incidents);
            return res;
        }
    }
}
