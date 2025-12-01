using System;
using System.Threading.Tasks;
using AVMonitoring.Functions.Services;
using Microsoft.Azure.Functions.Worker;

namespace AVMonitoring.Functions.Functions
{
    public class CleanupLogs
    {
        private readonly MonitoringLogRepository _repo;

        public CleanupLogs(MonitoringLogRepository repo)
        {
            _repo = repo;
        }

        [Function("CleanupLogs")]
        public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timer)
        {
            await _repo.DeleteAllAsync();
        }
    }
}
