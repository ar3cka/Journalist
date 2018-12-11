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
                batch => CloudEntity.ExecuteBatchAsync(batch, null, null),
                batch => CloudEntity.ExecuteBatch(batch),
                m_tableEntityConverter);
        }

        public ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string rowKey, string[] properties)
        {
            Require.NotNull(partitionKey, "partitionKey");
            Require.NotNull(rowKey, "rowKey");
            Require.NotNull(properties, "properties");

            return PrepareEntityPointQuery(partitionKey, rowKey, properties, null);
        }

        public ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string rowKey, string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(partitionKey, "partitionKey");
            Require.NotNull(rowKey, "rowKey");
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTablePointQuery(
                partitionKey: partitionKey,
                rowKey: rowKey,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string[] properties)
        {
            Require.NotNull(partitionKey, "partitionKey");
            Require.NotNull(properties, "properties");

            return PrepareEntityPointQuery(partitionKey, properties, null);
        }

        public ICloudTableEntityQuery PrepareEntityPointQuery(string partitionKey, string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(partitionKey, "partitionKey");
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTablePointQuery(
                partitionKey: partitionKey,
                rowKey: string.Empty,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntityRangeQuery PrepareEntityFilterRangeQuery(string filter, string[] properties)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            return PrepareEntityFilterRangeQuery(filter, properties, null);
        }

        public ICloudTableEntityRangeQuery PrepareEntityFilterRangeQuery(string filter, string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTableFilterRangeQuery(
                filter: filter,
                take: null,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, string[] properties)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            return PrepareEntityFilterSegmentedRangeQuery(filter, properties, null);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTableFilterSegmentedRangeQuery(
                filter: filter,
                take: null,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, int count, string[] properties)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            return PrepareEntityFilterSegmentedRangeQuery(filter, count, properties, null);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityFilterSegmentedRangeQuery(string filter, int count, string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(filter, "filter");
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTableFilterSegmentedRangeQuery(
                filter: filter,
                take: count,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntityRangeQuery PrepareEntityGetAllQuery(string[] properties)
        {
            Require.NotNull(properties, "properties");

            return PrepareEntityGetAllQuery(properties, null);
        }

        public ICloudTableEntityRangeQuery PrepareEntityGetAllQuery(string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTableFilterRangeQuery(
                filter: null,
                take: null,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(string[] properties)
        {
            Require.NotNull(properties, "properties");

            return PrepareEntityGetAllSegmentedQuery(properties, null);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTableFilterSegmentedRangeQuery(
                filter: null,
                take: null,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(int count, string[] properties)
        {
            Require.NotNull(properties, "properties");

            return PrepareEntityGetAllSegmentedQuery(count, properties, null);
        }

        public ICloudTableEntitySegmentedRangeQuery PrepareEntityGetAllSegmentedQuery(int count, string[] properties, Action<ITableRequestOptions> setupOptions)
        {
            Require.NotNull(properties, "properties");

            TableRequestOptions requestOptions = null;
            if (setupOptions != null)
            {
                var adapter = new TableRequestOptionsAdapter();
                setupOptions(adapter);
                requestOptions = adapter.Options;
            }

            return new CloudTableFilterSegmentedRangeQuery(
                filter: null,
                take: count,
                properties: properties,
                fetchAsync: ExecuteQueryAsync,
                fetchSync: ExecuteQuerySync,
                requestOptions: requestOptions,
                tableEntityConverter: m_tableEntityConverter);
        }

        private Task<TableQuerySegment<DynamicTableEntity>> ExecuteQueryAsync(
            TableQuery<DynamicTableEntity> query,
            TableContinuationToken continuationToken,
            TableRequestOptions requestOptions)
        {
            return CloudEntity.ExecuteQuerySegmentedAsync(query, continuationToken, requestOptions, null);
        }

        private TableQuerySegment<DynamicTableEntity> ExecuteQuerySync(
            TableQuery<DynamicTableEntity> query,
            TableContinuationToken continuationToken,
            TableRequestOptions requestOptions)
        {
            return CloudEntity.ExecuteQuerySegmented(query, continuationToken, requestOptions);
        }
    }
}
