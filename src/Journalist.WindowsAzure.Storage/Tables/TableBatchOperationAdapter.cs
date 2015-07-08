using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Journalist.Collections;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public class TableBatchOperationAdapter : IBatchOperation
    {
        private const int MAX_OPERATIONS_COUNT = 100;

        private static readonly ILogger s_logger = Log.ForContext<TableBatchOperationAdapter>();

        private readonly Func<TableBatchOperation, Task<IList<TableResult>>> m_executeBatch;
        private readonly TableBatchOperation m_batch;
        private readonly ITableEntityConverter m_tableEntityConverter;

        public TableBatchOperationAdapter(
            Func<TableBatchOperation, Task<IList<TableResult>>> executeBatch,
            ITableEntityConverter tableEntityConverter)
        {
            Require.NotNull(executeBatch, "executeBatch");
            Require.NotNull(tableEntityConverter, "tableEntityConverter");

            m_executeBatch = executeBatch;
            m_tableEntityConverter = tableEntityConverter;
            m_batch = new TableBatchOperation();
        }

        public void Insert(string partitionKey, string rowKey, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;

            m_batch.Insert(entity);
        }

        public void Insert(string partitionKey, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = string.Empty;

            m_batch.Insert(entity);
        }

        public void Merge(string partitionKey, string rowKey, string etag, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");
            Require.NotEmpty(etag, "etag");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;
            entity.ETag = etag;

            m_batch.Merge(entity);
        }

        public void Merge(string partitionKey, string etag, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(etag, "etag");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = string.Empty;
            entity.ETag = etag;

            m_batch.Merge(entity);
        }

        public void Replace(string partitionKey, string rowKey, string etag, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");
            Require.NotEmpty(etag, "etag");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;
            entity.ETag = etag;

            m_batch.Replace(entity);
        }

        public void Replace(string partitionKey, string etag, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(etag, "etag");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = string.Empty;
            entity.ETag = etag;

            m_batch.Replace(entity);
        }

        public void InsertOrMerge(string partitionKey, string rowKey, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;

            m_batch.InsertOrMerge(entity);
        }

        public void InsertOrMerge(string partitionKey, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = string.Empty;

            m_batch.InsertOrMerge(entity);
        }

        public void InsertOrReplace(string partitionKey, string rowKey, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;

            m_batch.InsertOrReplace(entity);
        }

        public void InsertOrReplace(string partitionKey, IReadOnlyDictionary<string, object> properties)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotNull(properties, "properties");

            AssertBatchSizeIsNotExceeded();

            var entity = m_tableEntityConverter.CreateDynamicTableEntityFromProperties(properties);
            entity.PartitionKey = partitionKey;
            entity.RowKey = string.Empty;

            m_batch.InsertOrReplace(entity);
        }

        public void Delete(string partitionKey, string rowKey, string etag)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotEmpty(rowKey, "rowKey");
            Require.NotNull(etag, "etag");

            AssertBatchSizeIsNotExceeded();

            var entity = new DynamicTableEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                ETag = etag
            };

            m_batch.Delete(entity);
        }

        public void Delete(string partitionKey, string etag)
        {
            Require.NotEmpty(partitionKey, "partitionKey");
            Require.NotNull(etag, "etag");

            AssertBatchSizeIsNotExceeded();

            var entity = new DynamicTableEntity
            {
                PartitionKey = partitionKey,
                RowKey = string.Empty,
                ETag = etag
            };

            m_batch.Delete(entity);
        }

        public async Task<IReadOnlyList<OperationResult>> ExecuteAsync()
        {
            try
            {
                var tableResult = await m_executeBatch(m_batch);

                var result = new List<OperationResult>(tableResult.Count);
                result.AddRange(tableResult.Select(r => new OperationResult(
                    r.Etag ?? string.Empty, (HttpStatusCode) r.HttpStatusCode)));

                return result;
            }
            catch (StorageException exception)
            {
                s_logger.Verbose(exception, "Execution of batch operation failed.");

                if (exception.RequestInformation.ExtendedErrorInformation == null)
                {
                    throw;
                }

                var errorInfo = exception.RequestInformation
                    .ExtendedErrorInformation
                    .ErrorMessage.Split("\n".YieldArray(), StringSplitOptions.RemoveEmptyEntries);

                var operationInfo = errorInfo[0].Split(':'.YieldArray(), StringSplitOptions.RemoveEmptyEntries);

                int operationNumber;
                int.TryParse(operationInfo[0], out operationNumber);

                throw new BatchOperationException(
                    operationBatchNumber: operationNumber,
                    statusCode: (HttpStatusCode) exception.RequestInformation.HttpStatusCode,
                    operationErrorCode: exception.RequestInformation.ExtendedErrorInformation.ErrorCode,
                    inner: exception);
            }
        }

        private void AssertBatchSizeIsNotExceeded()
        {
            if ((OperationsCount + 1) > MAX_OPERATIONS_COUNT)
            {
                throw new InvalidOperationException("Max batch size was exceeded.");
            }
        }

        public int OperationsCount
        {
            get { return m_batch.Count; }
        }

        public bool IsMaximumOperationsCountWasReached
        {
            get { return m_batch.Count == MAX_OPERATIONS_COUNT; }
        }
    }
}
