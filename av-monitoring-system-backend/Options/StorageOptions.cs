namespace AVMonitoring.Functions.Options
{
    public class StorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string MonitoringTable { get; set; } = "MonitoringLogs";

        public string EndpointsTable { get; set; } = "Endpoints";
        public string ErrorContainer { get; set; } = "errors";
    }
}
