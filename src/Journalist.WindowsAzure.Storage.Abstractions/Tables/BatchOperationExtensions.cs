using Journalist.Collections;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public static class BatchOperationExtensions
    {
        public static void Insert(this IBatchOperation batchOperation, string partitionKey, string rowKey)
        {
            Require.NotNull(batchOperation, "batchOperation");

            batchOperation.Insert(partitionKey, rowKey, EmptyDictionary.Get<string, object>());
        }

        public static void InsertOrMerge(this IBatchOperation batchOperation, string partitionKey, string rowKey)
        {
            Require.NotNull(batchOperation, "batchOperation");

            batchOperation.InsertOrMerge(partitionKey, rowKey, EmptyDictionary.Get<string, object>());
        }

        public static void InsertOrReplace(this IBatchOperation batchOperation, string partitionKey, string rowKey)
        {
            Require.NotNull(batchOperation, "batchOperation");

            batchOperation.InsertOrReplace(partitionKey, rowKey, EmptyDictionary.Get<string, object>());
        }

        public static void Merge(this IBatchOperation batchOperation, string partitionKey, string rowKey, string etag)
        {
            Require.NotNull(batchOperation, "batchOperation");

            batchOperation.Merge(partitionKey, rowKey, etag, EmptyDictionary.Get<string, object>());
        }

        public static void Replace(this IBatchOperation batchOperation, string partitionKey, string rowKey, string etag)
        {
            Require.NotNull(batchOperation, "batchOperation");

            batchOperation.Replace(partitionKey, rowKey, etag, EmptyDictionary.Get<string, object>());
        }
    }
}
