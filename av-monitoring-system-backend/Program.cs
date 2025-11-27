using AVMonitoring.Functions.Options;
using AVMonitoring.Functions.Services;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.UseMiddleware<CorsMiddleware>();

// Bind strongly typed configuration
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

// HttpClient for service pinging
builder.Services.AddHttpClient("MonitoringHttpClient")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

// TableServiceClient via Options
builder.Services.AddSingleton<TableServiceClient>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
    return new TableServiceClient(opts.ConnectionString);
});

// BlobServiceClient via Options
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
    return new BlobServiceClient(opts.ConnectionString);
});

// BlobContainerClient for error logs
builder.Services.AddSingleton<BlobContainerClient>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
    var svc = sp.GetRequiredService<BlobServiceClient>();
    return svc.GetBlobContainerClient(opts.ErrorContainer);
});

builder.Services.AddSingleton<IHttpPingService, HttpPingService>();
builder.Services.AddSingleton<EndpointRepository>();
builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddSingleton<MonitoringLogRepository>();
builder.Services.AddSingleton<IncidentRepository>();
builder.Services.AddSingleton<IncidentEngine>();

builder.Build().Run();
