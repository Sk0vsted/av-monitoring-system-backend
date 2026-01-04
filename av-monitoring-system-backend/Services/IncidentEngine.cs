using AVMonitoring.Functions.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Services
{
    public class IncidentEngine
    {
        private readonly IncidentRepository _incidentRepo;
        public IncidentEngine(IncidentRepository incidentRepo)
        {
            _incidentRepo = incidentRepo;
        }
        // Public API
        public async Task<List<IncidentRecord>> AnalyzeAsync(string partitionKey, List<MonitoringLogEntity> logs)
        {
            var incidents = new List<IncidentRecord>();

            if (logs.Count < 5)
                return incidents;

            logs = logs
                .OrderBy(l => l.PingedAtUtc)
                .ToList();

            // HTTP ERROR (DEDUP)
            var httpError = await TryHttpErrorAsync(logs, partitionKey);
            if (httpError != null)
            {
                incidents.Add(httpError);
                return incidents;
            }

            // LATENCY (NO DEDUP)
            if (TryLatencyRegression(logs, out var latencyIncident))
                incidents.Add(latencyIncident);

            // FLAPPING (NO DEDUP)
            if (TryFlapping(logs, out var flappingIncident))
                incidents.Add(flappingIncident);

            return incidents;
        }

        // Incident #0: HTTP Error
        private async Task<IncidentRecord?> TryHttpErrorAsync(List<MonitoringLogEntity> logs, string partitionKey)
        {
            var last = logs.Last();

            bool isError =
                last.IsError ||
                last.StatusCode < 200 ||
                last.StatusCode >= 400;

            if (!isError)
                return null;

            bool alreadyExists = await _incidentRepo.HasIncidentOfTypeAsync(partitionKey, "HttpError");
            if (alreadyExists)
                return null;

            return new IncidentRecord
            {
                IncidentType = "HttpError",
                Severity = last.StatusCode >= 500 || last.StatusCode == 0 ? "Critical" : "Warning",
                Message = $"HTTP error detected: Status {last.StatusCode}.",
                ValueNow = last.StatusCode,
                ValueBaseline = 200,
                CreatedUtc = DateTime.UtcNow
            };
        }

        //Incident #1: Latency Regression
        private bool TryLatencyRegression(List<MonitoringLogEntity> logs, out IncidentRecord inc)
        {
            inc = null;

            var last5 = logs.TakeLast(5).ToList();
            var latest = last5.Last().ResponseTimeMs;

            var baseline = CalculateDynamicBaseline(logs);
            if (baseline <= 0)
                return false;

            var (warningThreshold, criticalThreshold) =
                CalculateLatencyThresholds(baseline);

            if (latest < warningThreshold)
                return false;

            int trending = last5.Count(l => l.ResponseTimeMs > warningThreshold);
            if (trending < 3)
                return false;

            inc = new IncidentRecord
            {
                IncidentType = "LatencyRegression",
                Severity = latest > criticalThreshold ? "Critical" : "Warning",
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

        private double CalculateDynamicBaseline(List<MonitoringLogEntity> logs)
        {
            var stableLogs = logs
                .Where(l => !l.IsError && l.ResponseTimeMs > 0)
                .TakeLast(20)
                .Select(l => (double)l.ResponseTimeMs)
                .ToList();

            if (stableLogs.Count < 5)
                return 0;

            return Median(stableLogs);
        }

        private (double warning, double critical) CalculateLatencyThresholds(double baseline)
        {
            if (baseline <= 0)
                return (0, 0);

            // Hurtige endpoints = strengere
            if (baseline < 300)
                return (baseline * 1.8, baseline * 3.0);

            // Langsommere endpoints = mere tolerance
            return (baseline * 1.5, baseline * 2.5);
        }
    }
}

