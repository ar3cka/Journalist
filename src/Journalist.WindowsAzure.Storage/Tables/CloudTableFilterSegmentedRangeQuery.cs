using System;
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
            Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>> fetchEntities,
            ITableEntityConverter tableEntityConverter)
            : base(take, properties, fetchEntities, tableEntityConverter)
        {
            m_filter = filter;
        }

        public Task<List<Dictionary<string, object>>> ExecuteAsync()
        {
            return FetchEntities(m_filter, EmptyArray.Get<byte>());
        }

        public Task<List<Dictionary<string, object>>> ExecuteAsync(byte[] continuationToken)
        {
            Require.NotNull(continuationToken, "continuationToken");

            return FetchEntities(m_filter, continuationToken);
        }

        public bool HasMore
        {
            get
            {
                return ReadNextSegment;
            }
        }

        public byte[] ContinuationToken
        {
            get
            {
                return GetContinuationTokenBytes();
            }
        }
    }
}
