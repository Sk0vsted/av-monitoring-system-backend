using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AVMonitoring.Functions.Models;

namespace AVMonitoring.Functions.Services
{
    public class IncidentEngine
    {
        // Public API
        public List<IncidentRecord> Analyze(List<MonitoringLogEntity> logs)
        {
            var incidents = new List<IncidentRecord>();

            if (logs.Count < 5)
                return incidents;

            logs = logs
                .OrderBy(l => l.PingedAtUtc)
                .ToList();

            if (TryHttpError(logs, out var httpErrorIncident))
            {
                incidents.Add(httpErrorIncident);
                return incidents;
            }

            if (TryLatencyRegression(logs, out var latencyIncident))
                incidents.Add(latencyIncident);

            if (TryFlapping(logs, out var flappingIncident))
                incidents.Add(flappingIncident);

            return incidents;
        }

        // Incident #0: HTTP Error
        private bool TryHttpError(List<MonitoringLogEntity> logs, out IncidentRecord incident)
        {
            incident = null;

            var last = logs.Last();

            // Error conditions
            bool isError =
                last.IsError ||
                last.StatusCode < 200 ||
                last.StatusCode >= 400;

            if (!isError)
                return false;

            incident = new IncidentRecord
            {
                IncidentType = "HttpError",
                Severity = last.StatusCode >= 500 || last.StatusCode == 0
                    ? "Critical"
                    : "Warning",
                Message = $"HTTP error detected: Status {last.StatusCode}.",
                ValueNow = last.StatusCode,
                ValueBaseline = 200,
                CreatedUtc = DateTime.UtcNow
            };

            return true;
        }


        //Incident #1: Latency Regression
        private bool TryLatencyRegression(List<MonitoringLogEntity> logs, out IncidentRecord inc)
        {
            inc = null;

            var last5 = logs.TakeLast(5).ToList();
            var baseline = Median(last5.Take(4).Select(x => (double)x.ResponseTimeMs).ToList());
            var latest = last5.Last().ResponseTimeMs;

            if (baseline < 200)
                return false;

            if (latest < 1000)
                return false;

            var pct = ((latest - baseline) / baseline) * 100.0;

            if (latest < baseline * 2.0)
                return false;

            int trending = last5.Count(l => l.ResponseTimeMs > baseline * 1.5);
            if (trending < 3)
                return false;

            inc = new IncidentRecord
            {
                IncidentType = "LatencyRegression",
                Severity = latest > baseline * 4 ? "Critical" : "Warning",
                Message = $"Latency regression: baseline {baseline:F0}ms → {latest:F0}ms.",
                ValueNow = latest,
                ValueBaseline = baseline,
                CreatedUtc = DateTime.UtcNow
            };

            return true;
        }

        // Incident #2: Flapping / Instability
        private bool TryFlapping(List<MonitoringLogEntity> logs, out IncidentRecord inc)
        {
            inc = null;

            var last5 = logs.TakeLast(5).ToList();
            int errors = last5.Count(l => l.IsError || l.StatusCode >= 500);

            if (errors < 4)
                return false;

            inc = new IncidentRecord
            {
                IncidentType = "Flapping",
                Severity = "Warning",
                Message = $"Endpoint unstable: {errors}/5 failed.",
                ValueNow = errors,
                ValueBaseline = 5,
                CreatedUtc = DateTime.UtcNow
            };

            return true;
        }

        private double Median(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0;

            values.Sort();
            int mid = values.Count / 2;
            return values.Count % 2 == 0
                ? (values[mid - 1] + values[mid]) / 2.0
                : values[mid];
        }
    }
}

