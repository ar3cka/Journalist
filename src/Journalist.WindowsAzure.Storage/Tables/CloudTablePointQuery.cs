using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class CloudTablePointQuery : CloudTableSegmentedQuery, ICloudTableEntityQuery
    {
        private readonly string m_partitionKey;
        private readonly string m_rowKey;

        public CloudTablePointQuery(
            string partitionKey,
            string rowKey,
            string[] properties,
            Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>> fetchEntities,
            ITableEntityConverter tableEntityConverter)
            : base(1, properties, fetchEntities, tableEntityConverter)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotNull(rowKey, "rowKey");

            m_partitionKey = partitionKey;
            m_rowKey = rowKey;
        }

        public async Task<Dictionary<string, object>> ExecuteAsync()
        {
            var entities = await FetchEntities(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, m_partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, m_rowKey)));

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
    }
}
