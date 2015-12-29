using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public sealed class CloudTableFilterRangeQuery : CloudTableSegmentedQuery, ICloudTableEntityRangeQuery
    {
        private readonly string m_filter;

        public CloudTableFilterRangeQuery(
            string filter,
            int? take,
            string[] properties,
            Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>> fetchEntities,
            ITableEntityConverter tableEntityConverter)
            : base(take, properties, fetchEntities, tableEntityConverter)
        {
            m_filter = filter;
        }

        public async Task<List<Dictionary<string, object>>> ExecuteAsync()
        {
            var result = new List<Dictionary<string, object>>();

            do
            {
                result.AddRange(await FetchEntities(m_filter, EmptyArray.Get<byte>()));
            }
            while (ReadNextSegment);

            return result;
        }
    }
}
