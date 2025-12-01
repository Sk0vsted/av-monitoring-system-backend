using AVMonitoring.Functions.Models;
using AVMonitoring.Functions.Options;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AVMonitoring.Functions.Services
{
    public class IncidentRepository
    {
        private readonly TableClient _table;

        public IncidentRepository(IOptions<StorageOptions> opts, TableServiceClient svc)
        {
            _table = svc.GetTableClient(opts.Value.IncidentsTable);
            _table.CreateIfNotExists();
        }

        public async Task SaveAsync(IncidentEntity entity)
        {
            await _table.AddEntityAsync(entity);
        }

        public async Task<IncidentEntity?> GetByIdAsync(string partitionKey, string rowKey)
        {
            try
            {
                var result = await _table.GetEntityAsync<IncidentEntity>(partitionKey, rowKey);
                return result.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<List<IncidentEntity>> GetRecentAsync(int limit = 20)
        {
            var list = new List<IncidentEntity>();

            await foreach (var entity in _table.QueryAsync<IncidentEntity>())
            {
                list.Add(entity);
            }

            return list
                .OrderByDescending(x => x.CreatedUtc)
                .Take(limit)
                .ToList();
        }

        public async Task DeleteAllAsync()
        {
            var partitionKeys = new HashSet<string>();

            await foreach (var entity in _table.QueryAsync<IncidentEntity>())
            {
                partitionKeys.Add(entity.PartitionKey);
            }

            foreach (var pk in partitionKeys)
            {
                var batch = new List<TableTransactionAction>();

                await foreach (var entity in _table.QueryAsync<IncidentEntity>(e => e.PartitionKey == pk))
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
        }
        public async Task DeleteAsync(string partitionKey, string rowKey)
        {
            await _table.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<List<IncidentEntity>> GetAllAsync()
        {
            var list = new List<IncidentEntity>();

            await foreach (var entity in _table.QueryAsync<IncidentEntity>())
            {
                list.Add(entity);
            }

            return list
                .OrderByDescending(x => x.CreatedUtc)
                .ToList();
        }
    }
}
