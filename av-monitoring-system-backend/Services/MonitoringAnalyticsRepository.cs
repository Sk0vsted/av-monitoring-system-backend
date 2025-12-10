using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Options;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace AVMonitoring.Functions.Services;

public class MonitoringAnalyticsRepository
{
    private readonly TableClient _table;

    public MonitoringAnalyticsRepository(
        IOptions<StorageOptions> opts,
        TableServiceClient svc)
    {
        _table = svc.GetTableClient(opts.Value.AnalyticsTable);
        _table.CreateIfNotExists();
    }

    public async Task SaveDailyAnalyticsAsync(DailyAnalyticsEntity entity)
    {
        await _table.UpsertEntityAsync(entity);
    }

    public async Task<List<DailyAnalyticsEntity>> GetAllAsync()
    {
        var list = new List<DailyAnalyticsEntity>();

        await foreach (var item in _table.QueryAsync<DailyAnalyticsEntity>())
            list.Add(item);

        return list
            .OrderBy(x => x.Date)
            .ToList();
    }

}
