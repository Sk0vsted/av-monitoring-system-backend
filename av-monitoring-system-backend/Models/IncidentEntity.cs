using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Models
{
    public class IncidentEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom fields herunder
        public string EndpointName { get; set; } = default!;
        public string IncidentType { get; set; } = default!;
        public string Severity { get; set; } = default!;
        public string Message { get; set; } = default!;
        public double ValueNow { get; set; }
        public double ValueBaseline { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

}
