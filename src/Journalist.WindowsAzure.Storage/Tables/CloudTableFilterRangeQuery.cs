using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public sealed class CloudTableFilterRangeQuery : CloudTableQuery, ICloudTableEntityRangeQuery
    {
        private readonly string m_filter;

        public CloudTableFilterRangeQuery(
            string filter,
            int? take,
            string[] properties,
            Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>>
                fetchEntities,
            ITableEntityConverter tableEntityConverter)
            : base(take, properties, fetchEntities, tableEntityConverter)
        {
            Require.NotEmpty(filter, "filter");

            m_filter = filter;
        }

        public async Task<IList<IDictionary<string, object>>> ExecuteAsync()
        {
            return await FetchEntities(m_filter);
        }
    }
}