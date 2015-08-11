using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public abstract class CloudTableSegmentedQuery
    {
        private static readonly char[] s_separators = { ' ' };

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

        protected async Task<List<Dictionary<string, object>>> FetchEntities(string filter, byte[] continuationToken)
        {
            Require.NotEmpty(filter, "filter");
            Require.NotNull(continuationToken, "continuationToken");

            var query = new TableQuery<DynamicTableEntity>()
                .Select(m_properties)
                .Where(filter);

            List<Dictionary<string, object>> result;
            if (m_take.HasValue)
            {
                query = query.Take(m_take.Value);
                result = new List<Dictionary<string, object>>(m_take.Value);
            }
            else
            {
                result = new List<Dictionary<string, object>>();
            }

            if (!m_executionStarted && continuationToken.Any())
            {
                m_continuationToken = ParseContinuationTokenBytes(continuationToken);
            }

            var queryResult = await m_fetchEntities(query, m_continuationToken);
            result.AddRange(queryResult.Results.Select(m_tableEntityConverter.CreatePropertiesFromDynamicTableEntity));

            m_continuationToken = queryResult.ContinuationToken;
            m_executionStarted = true;

            return result;
        }

        protected byte[] GetContinuationTokenBytes()
        {
            if (m_continuationToken == null)
            {
                return EmptyArray.Get<byte>();
            }

            return Encoding.UTF8.GetBytes(
                "{0} {1} {2} {3}".FormatString(
                    m_continuationToken.NextPartitionKey ?? string.Empty,
                    m_continuationToken.NextRowKey ?? string.Empty,
                    m_continuationToken.NextTableName ?? string.Empty,
                    m_continuationToken.TargetLocation));
        }

        private TableContinuationToken ParseContinuationTokenBytes(byte[] continuationTokenBytes)
        {
            var parts = Encoding.UTF8.GetString(continuationTokenBytes).Split(s_separators);

            Ensure.True<InvalidOperationException>(parts.Length == 4, "Invalid continuation token format.");

            return new TableContinuationToken
            {
                NextPartitionKey = parts[0],
                NextRowKey = parts[1],
                NextTableName = parts[2],
                TargetLocation = string.IsNullOrEmpty(parts[3])
                    ? (StorageLocation?)null
                    : (StorageLocation)Enum.Parse(typeof(StorageLocation), parts[3])
            };
        }

        protected bool ReadNextSegment
        {
            get
            {
                return m_executionStarted && m_continuationToken != null;
            }
        }
    }
}
