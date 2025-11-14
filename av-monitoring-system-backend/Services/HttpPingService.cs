using System.Diagnostics;
using System.Net.Http;
using AVMonitoring.Functions.Models;

namespace AVMonitoring.Functions.Services;

public class HttpPingService : IHttpPingService
{
    private readonly HttpClient _http;

    public HttpPingService(IHttpClientFactory httpFactory)
    {
        _http = httpFactory.CreateClient("MonitoringHttpClient");
    }

    public async Task<PingResult> PingAsync(string url)
    {
        var watch = Stopwatch.StartNew();
        int status = 0;
        string? error = null;

        try
        {
            var response = await _http.GetAsync(url);
            status = (int)response.StatusCode;
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        watch.Stop();

        return new PingResult
        {
            Url = url,
            ResponseTimeMs = watch.ElapsedMilliseconds,
            StatusCode = status,
            IsError = error != null,
            ErrorMessage = error,
            Timestamp = DateTime.UtcNow
        };
    }
}
