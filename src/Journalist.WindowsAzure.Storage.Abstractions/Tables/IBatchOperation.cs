using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface IBatchOperation
    {
        void Insert(string partitionKey, string rowKey, IReadOnlyDictionary<string, object> properties);

        void Insert(string partitionKey, IReadOnlyDictionary<string, object> properties);

        void Merge(string partitionKey, string rowKey, string etag, IReadOnlyDictionary<string, object> properties);

        void Merge(string partitionKey, string etag, IReadOnlyDictionary<string, object> properties);

        void Replace(string partitionKey, string rowKey, string etag, IReadOnlyDictionary<string, object> properties);

        void Replace(string partitionKey, string etag, IReadOnlyDictionary<string, object> properties);

        void InsertOrReplace(string partitionKey, string rowKey, IReadOnlyDictionary<string, object> properties);

        void InsertOrReplace(string partitionKey, IReadOnlyDictionary<string, object> properties);

        void InsertOrMerge(string partitionKey, string rowKey, IReadOnlyDictionary<string, object> properties);

        void InsertOrMerge(string partitionKey, IReadOnlyDictionary<string, object> properties);

        void Delete(string partitionKey, string rowKey, string etag);

        void Delete(string partitionKey, string etag);

        Task<IReadOnlyList<OperationResult>> ExecuteAsync();

        int OperationsCount { get; }

        bool IsMaximumOperationsCountWasReached { get; }
    }
}
