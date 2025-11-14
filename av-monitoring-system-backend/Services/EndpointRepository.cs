using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Options;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace AVMonitoring.Functions.Services;

public class EndpointRepository
{
    private readonly TableClient _table;

    public EndpointRepository(IOptions<StorageOptions> opts, TableServiceClient svc)
    {
        _table = svc.GetTableClient(opts.Value.EndpointsTable);
        _table.CreateIfNotExists();
    }

    public Task AddAsync(MonitoredEndpointEntity entity)
        => _table.AddEntityAsync(entity);

    public Task UpsertAsync(MonitoredEndpointEntity entity)
        => _table.UpsertEntityAsync(entity);

    public async Task<List<MonitoredEndpointEntity>> GetAllAsync()
    {
        var list = new List<MonitoredEndpointEntity>();
        await foreach (var item in _table.QueryAsync<MonitoredEndpointEntity>())
            list.Add(item);

        return list;
    }
}

