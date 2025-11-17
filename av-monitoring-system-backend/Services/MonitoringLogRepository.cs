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

    public async Task DeleteOlderThanAsync(DateTime cutoff)
    {
        // Table Storage supports up to 100 operations per transaction
        var batch = new List<TableTransactionAction>();
        var cutoffTicks = cutoff.Ticks.ToString();

        await foreach (var entity in _table.QueryAsync<MonitoringLogEntity>(
            x => string.Compare(x.RowKey, cutoffTicks) < 0))
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));

            if (batch.Count == 100)
            {
                await _table.SubmitTransactionAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await _table.SubmitTransactionAsync(batch);
        }
    }

    public async Task<List<MonitoringLogEntity>> GetAllAsync()
    {
        var list = new List<MonitoringLogEntity>();

        await foreach (var item in _table.QueryAsync<MonitoringLogEntity>())
            list.Add(item);

        return list
            .OrderByDescending(x => x.PingedAtUtc)
            .ToList();
    }
}
