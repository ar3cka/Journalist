using System;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Internals;
using Journalist.WindowsAzure.Storage.Tables.TableEntityConverters;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class CloudTableAdapter : CloudEntityAdapter<CloudTable>, ICloudTable
    {
        private readonly ITableEntityConverter m_tableEntityConverter;

        internal CloudTableAdapter(Func<CloudTable> tableFactory) : base(tableFactory)
        {
            Require.NotNull(tableFactory, "tableFactory");

            m_tableEntityConverter = new TableEntityConverter();
        }

        public IBatchOperation PrepareBatchOperation()
        {
            return new TableBatchOperationAdapter(
                batch => CloudEntity.ExecuteBatchAsync(batch),
                m_tableEntityConverter);
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

        public ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string[] properties)
        {
            Require.NotNull(partitionKey, "partitionKey");
            Require.NotNull(properties, "properties");

            return new CloudTablePointQuery(
                partitionKey: partitionKey,
                rowKey: string.Empty,
                properties: properties,
                fetchEntities: ExecuteQueryAsync,
                tableEntityConverter: m_tableEntityConverter);
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

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, string[] properties)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            return new CloudTableFilterSegmentedRangeQuery(
                filter: filter,
                take: null,
                properties: properties,
                fetchEntities: ExecuteQueryAsync,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntityRangeQuery PrepareEntityGetAllQuery(string[] properties)
        {
            Require.NotNull(properties, "properties");

            return new CloudTableFilterRangeQuery(
                filter: null,
                take: null,
                properties: properties,
                fetchEntities: ExecuteQueryAsync,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(string[] properties)
        {
            Require.NotNull(properties, "properties");

            return new CloudTableFilterSegmentedRangeQuery(
                filter: null,
                take: null,
                properties: properties,
                fetchEntities: ExecuteQueryAsync,
                tableEntityConverter: m_tableEntityConverter);
        }

        private Task<TableQuerySegment<DynamicTableEntity>> ExecuteQueryAsync(TableQuery<DynamicTableEntity> query,
            TableContinuationToken continuationToken)
        {
            return CloudEntity.ExecuteQuerySegmentedAsync(query, continuationToken);
        }
    }
}
