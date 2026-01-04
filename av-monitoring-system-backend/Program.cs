using AVMonitoring.Functions.Options;
using AVMonitoring.Functions.Services;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder => // <-- Kritisk: dette er hele “isolated worker” modellen
    {
        builder.UseMiddleware<JwtAuthMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // Bind strongly typed storage settings
        services.Configure<StorageOptions>(config.GetSection("Storage"));

        // HttpClient til ping
        services.AddHttpClient("MonitoringHttpClient")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddSingleton<JwtTokenValidator>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var domain = config.GetValue<string>("Auth0:Domain");
            var audience = config.GetValue<string>("Auth0:Audience");

            return new JwtTokenValidator(domain, audience);
        });

        // TableServiceClient
        services.AddSingleton<TableServiceClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            return new TableServiceClient(opts.ConnectionString);
        });

        // BlobServiceClient
        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            return new BlobServiceClient(opts.ConnectionString);
        });

        // BlobContainerClient for errors
        services.AddSingleton<BlobContainerClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var svc = sp.GetRequiredService<BlobServiceClient>();
            return svc.GetBlobContainerClient(opts.ErrorContainer);
        });

        // Custom services
        services.AddSingleton<IHttpPingService, HttpPingService>();
        services.AddSingleton<EndpointRepository>();
        services.AddSingleton<MonitoringLogRepository>();
        services.AddSingleton<IncidentRepository>();
        services.AddSingleton<IncidentEngine>();
        services.AddSingleton<MonitoringService>();
        services.AddSingleton<MonitoringAnalyticsService>();
        services.AddSingleton<MonitoringAnalyticsRepository>();
    })
    .Build();

host.Run();
