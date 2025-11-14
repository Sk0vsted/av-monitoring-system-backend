using AVMonitoring.Functions.Services;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace AVMonitoring.Functions.Functions
{
    public class TestWriteLog
    {
        private readonly IHttpPingService _ping;

        public TestWriteLog(IHttpPingService ping)
        {
            _ping = ping;
        }

        [Function("TestWriteLog")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var result = await _ping.PingAsync("https://www.google.com");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}
