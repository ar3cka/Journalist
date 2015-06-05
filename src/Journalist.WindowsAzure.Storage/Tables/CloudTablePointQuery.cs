using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class CloudTablePointQuery : CloudTableQuery, ICloudTableEntityQuery
    {
        public CloudTablePointQuery(
            string partitionKey,
            string rowKey,
            string[] properties,
            Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>>
                fetchEntities,
            ITableEntityConverter tableEntityConverter)
            : base(1, properties, fetchEntities, tableEntityConverter)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");

            _partitionKey = partitionKey;
            _rowKey = rowKey;
        }

        public async Task<IDictionary<string, object>> ExecuteAsync()
        {
            var entities = await FetchEntities(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, _rowKey)));

            if (entities.Count == 0)
            {
                return null;
            }

            if (entities.Count > 1)
            {
                throw new InvalidOperationException("Single entity query returns to much rows.");
            }

            return entities[0];
        }

        private readonly string _partitionKey;
        private readonly string _rowKey;
    }
}