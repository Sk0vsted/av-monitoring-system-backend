using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Options;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace AVMonitoring.Functions.Services;

public class MonitoringLogRepository
{
    private readonly TableClient _table;

    public MonitoringLogRepository(IOptions<StorageOptions> opts, TableServiceClient svc)
    {
        _table = svc.GetTableClient(opts.Value.MonitoringTable);
        _table.CreateIfNotExists();
    }

    public Task AddAsync(MonitoringLogEntity entity)
        => _table.AddEntityAsync(entity);

    public async Task<List<MonitoringLogEntity>> GetByEndpointAsync(string endpointId)
    {
        var list = new List<MonitoringLogEntity>();

        await foreach (var item in _table.QueryAsync<MonitoringLogEntity>(x => x.PartitionKey == endpointId))
            list.Add(item);

        // sort by time newest first (Ticks in RowKey are good!)
        return list.OrderByDescending(x => x.PingedAtUtc).ToList();
    }
}
