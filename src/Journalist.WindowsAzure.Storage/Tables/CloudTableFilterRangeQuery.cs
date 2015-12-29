using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public sealed class CloudTableFilterRangeQuery : CloudTableSegmentedQuery, ICloudTableEntityRangeQuery
    {
        private readonly string m_filter;

        public CloudTableFilterRangeQuery(
            string filter,
            int? take,
            string[] properties,
            FetchAsync fetchAsync,
            FetchSync fetchSync,
            ITableEntityConverter tableEntityConverter)
            : base(take, properties, fetchAsync, fetchSync, tableEntityConverter)
        {
            m_filter = filter;
        }

        public async Task<List<Dictionary<string, object>>> ExecuteAsync()
        {
            var result = new List<Dictionary<string, object>>();

            do
            {
                result.AddRange(await FetchEntitiesAsync(m_filter, EmptyArray.Get<byte>()));
            }
            while (ReadNextSegment);

            return result;
        }

        public List<Dictionary<string, object>> Execute()
        {
            var result = new List<Dictionary<string, object>>();

            do
            {
                result.AddRange(FetchEntities(m_filter, EmptyArray.Get<byte>()));
            }
            while (ReadNextSegment);

            return result;
        }
    }
}
