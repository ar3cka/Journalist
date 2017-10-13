using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    internal class CloudTableFilterSegmentedRangeQuery : CloudTableSegmentedQuery, ICloudTableEntitySegmentedRangeQuery
    {
        private readonly string m_filter;

        public CloudTableFilterSegmentedRangeQuery(
            string filter,
            int? take,
            string[] properties,
            FetchAsync fetchAsync,
            FetchSync fetchSync,
            TableRequestOptions requestOptions,
            ITableEntityConverter tableEntityConverter)
            : base(take, properties, fetchAsync, fetchSync, requestOptions, tableEntityConverter)
        {
            m_filter = filter;
        }

        public Task<List<Dictionary<string, object>>> ExecuteAsync()
        {
            return FetchEntitiesAsync(m_filter, EmptyArray.Get<byte>());
        }

        public Task<List<Dictionary<string, object>>> ExecuteAsync(byte[] continuationToken)
        {
            Require.NotNull(continuationToken, "continuationToken");

            return FetchEntitiesAsync(m_filter, continuationToken);
        }

        public List<Dictionary<string, object>> Execute()
        {
            return FetchEntities(m_filter, EmptyArray.Get<byte>());
        }

        public List<Dictionary<string, object>> Execute(byte[] continuationToken)
        {
            Require.NotNull(continuationToken, "continuationToken");

            return FetchEntities(m_filter, continuationToken);
        }

        public bool HasMore => ReadNextSegment;

        public byte[] ContinuationToken => GetContinuationTokenBytes();
    }
}
