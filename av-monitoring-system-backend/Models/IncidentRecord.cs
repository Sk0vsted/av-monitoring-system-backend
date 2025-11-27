using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Models
{
    public class IncidentRecord
    {
        public string IncidentType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public double ValueNow { get; set; }
        public double ValueBaseline { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
