using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Tables.TableEntityConverters;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class CloudTableAdapter : ICloudTable
    {
        private readonly Lazy<CloudTable> m_table;
        private readonly ITableEntityConverter m_tableEntityConverter;

        internal CloudTableAdapter(Func<CloudTable> tableFactory)
        {
            Require.NotNull(tableFactory, "tableFactory");

            m_table = new Lazy<CloudTable>(tableFactory, LazyThreadSafetyMode.ExecutionAndPublication);
            m_tableEntityConverter = new TableEntityConverter();
        }

        public IBatchOperation PrepareBatchOperation()
        {
            return new TableBatchOperationAdapter(
                batch => Table.ExecuteBatchAsync(batch),
                m_tableEntityConverter);
        }

        public ICloudTableEntityRangeQuery PrepareEntityFilterRangeQuery(string filter, string[] properties)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            return new CloudTableFilterRangeQuery(
                filter: filter,
                take: null,
                properties: properties,
                fetchEntities: ExecuteQueryAsync,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string rowKey, string[] properties)
        {
            Require.NotNull(partitionKey, "partitionKey");
            Require.NotNull(rowKey, "rowKey");
            Require.NotNull(properties, "properties");

            return new CloudTablePointQuery(
                partitionKey: partitionKey,
                rowKey: rowKey,
                properties: properties,
                fetchEntities: ExecuteQueryAsync,
                tableEntityConverter: m_tableEntityConverter);
        }

        private Task<TableQuerySegment<DynamicTableEntity>> ExecuteQueryAsync(TableQuery<DynamicTableEntity> query,
            TableContinuationToken continuationToken)
        {
            return Table.ExecuteQuerySegmentedAsync(query, continuationToken);
        }

        private CloudTable Table
        {
            get { return m_table.Value; }
        }
    }
}