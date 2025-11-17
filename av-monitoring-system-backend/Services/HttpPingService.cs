using AVMonitoring.Functions.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AVMonitoring.Functions.Services;

public class HttpPingService : IHttpPingService
{
    private readonly HttpClient _client;

    public HttpPingService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("MonitoringHttpClient");
    }

    public async Task<PingResult> PingAsync(MonitoredEndpointEntity ep)
    {
        var request = new HttpRequestMessage(new HttpMethod(ep.Method), ep.Url);

        // Headers
        if (!string.IsNullOrWhiteSpace(ep.HeadersJson))
        {
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(ep.HeadersJson);
            foreach (var h in headers)
                request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        // Auth
        if (!string.IsNullOrWhiteSpace(ep.AuthHeader))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ep.AuthHeader);

        // Body
        if (!string.IsNullOrWhiteSpace(ep.BodyJson))
            request.Content = new StringContent(ep.BodyJson, Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await _client.SendAsync(request);

            sw.Stop();
            return new PingResult
            {
                StatusCode = (int)response.StatusCode,
                ResponseTimeMs = (int)sw.ElapsedMilliseconds,
                IsError = !response.IsSuccessStatusCode
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new PingResult
            {
                StatusCode = 0,
                ResponseTimeMs = (int)sw.ElapsedMilliseconds,
                IsError = true,
                ErrorMessage = ex.Message
            };
        }
    }
}
