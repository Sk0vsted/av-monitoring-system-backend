using System.Net;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AVMonitoring.Functions.Functions
{
    public class DeleteIncidentById
    {
        private readonly IncidentRepository _repo;

        public DeleteIncidentById(IncidentRepository repo)
        {
            _repo = repo;
        }

        [Function("DeleteIncidentById")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
            Route = "DeleteIncidentById/{partitionKey}/{rowKey}")]
            HttpRequestData req,
            string partitionKey,
            string rowKey)
        {
            var incident = await _repo.GetByIdAsync(partitionKey, rowKey);

            if (incident == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Incident not found.");
                return notFound;
            }

            await _repo.DeleteAsync(partitionKey, rowKey);

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
