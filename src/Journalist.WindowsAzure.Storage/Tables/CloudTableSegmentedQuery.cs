using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public abstract class CloudTableSegmentedQuery
    {
        private readonly int? m_take;
        private readonly string[] m_properties;
        private readonly Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>> m_fetchEntities;
        private readonly ITableEntityConverter m_tableEntityConverter;

        private TableContinuationToken m_continuationToken;
        private bool m_executionStarted;

        protected CloudTableSegmentedQuery(
            int? take,
            string[] properties,
            Func<TableQuery<DynamicTableEntity>, TableContinuationToken, Task<TableQuerySegment<DynamicTableEntity>>> fetchEntities,
            ITableEntityConverter tableEntityConverter)
        {
            Require.True(!take.HasValue || take > 0, "take", "Value should contains positive value");
            Require.NotNull(properties, "properties");
            Require.NotNull(fetchEntities, "fetchEntities");
            Require.NotNull(tableEntityConverter, "tableEntityConverter");

            m_take = take;
            m_properties = properties;
            m_fetchEntities = fetchEntities;
            m_tableEntityConverter = tableEntityConverter;
            m_continuationToken = null;
        }

        protected async Task<IList<IDictionary<string, object>>> FetchEntities(string filter)
        {
            Require.NotEmpty(filter, "filter");

            var query = new TableQuery<DynamicTableEntity>()
                .Select(m_properties)
                .Where(filter);

            List<IDictionary<string, object>> result;
            if (m_take.HasValue)
            {
                query = query.Take(m_take.Value);
                result = new List<IDictionary<string, object>>(m_take.Value);
            }
            else
            {
                result = new List<IDictionary<string, object>>();
            }

            var queryResult = await m_fetchEntities(query, m_continuationToken);
            result.AddRange(queryResult.Results.Select(m_tableEntityConverter.CreatePropertiesFromDynamicTableEntity));

            m_continuationToken = queryResult.ContinuationToken;
            m_executionStarted = true;

            return result;
        }

        protected bool ReadNextSegment
        {
            get { return m_executionStarted && m_continuationToken != null; }
        }
    }
}
