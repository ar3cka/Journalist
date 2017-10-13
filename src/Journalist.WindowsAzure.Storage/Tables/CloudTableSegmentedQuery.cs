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
    public delegate Task<TableQuerySegment<DynamicTableEntity>> FetchAsync(TableQuery<DynamicTableEntity> query, TableContinuationToken token, TableRequestOptions requestOptions);
    public delegate TableQuerySegment<DynamicTableEntity> FetchSync(TableQuery<DynamicTableEntity> query, TableContinuationToken token, TableRequestOptions requestOptions);

    public abstract class CloudTableSegmentedQuery
    {
        private static readonly char[] s_separators = { ' ' };

        private readonly int? m_take;
        private readonly string[] m_properties;
        private readonly FetchAsync m_fetchEntitiesAsync;
        private readonly FetchSync m_fetchEntities;
        private readonly TableRequestOptions m_requestOptions;
        private readonly ITableEntityConverter m_tableEntityConverter;

        private TableContinuationToken m_continuationToken;
        private bool m_executionStarted;

        protected CloudTableSegmentedQuery(
            int? take,
            string[] properties,
            FetchAsync fetchEntitiesAsync,
            FetchSync fetchEntities,
            TableRequestOptions requestOptions,
            ITableEntityConverter tableEntityConverter)
        {
            Require.True(!take.HasValue || take > 0, "take", "Value should contains positive value");
            Require.NotNull(properties, "properties");
            Require.NotNull(fetchEntitiesAsync, "fetchEntitiesAsync");
            Require.NotNull(fetchEntities, "fetchEntities");
            Require.NotNull(tableEntityConverter, "tableEntityConverter");

            m_take = take;
            m_properties = properties;
            m_fetchEntitiesAsync = fetchEntitiesAsync;
            m_fetchEntities = fetchEntities;
            m_requestOptions = requestOptions;
            m_tableEntityConverter = tableEntityConverter;
            m_continuationToken = null;
        }

        protected async Task<List<Dictionary<string, object>>> FetchEntitiesAsync(string filter, byte[] continuationToken)
        {
            Require.NotNull(continuationToken, "continuationToken");

            var query = PrepareQuery(filter);
            var result = AllocateResult();
            SetContinuationToken(continuationToken);

            var queryResult = await m_fetchEntitiesAsync(query, m_continuationToken, m_requestOptions);
            result.AddRange(queryResult.Results.Select(m_tableEntityConverter.CreatePropertiesFromDynamicTableEntity));

            UpdateContinuationToken(queryResult);

            return result;
        }

        protected List<Dictionary<string, object>> FetchEntities(string filter, byte[] continuationToken)
        {
            Require.NotNull(continuationToken, "continuationToken");

            var query = PrepareQuery(filter);
            var result = AllocateResult();
            SetContinuationToken(continuationToken);

            var queryResult = m_fetchEntities(query, m_continuationToken, m_requestOptions);
            result.AddRange(queryResult.Results.Select(m_tableEntityConverter.CreatePropertiesFromDynamicTableEntity));

            UpdateContinuationToken(queryResult);

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

        private void SetContinuationToken(byte[] continuationToken)
        {
            if (!m_executionStarted && continuationToken.Any())
            {
                m_continuationToken = ParseContinuationTokenBytes(continuationToken);
            }
        }

        private void UpdateContinuationToken(TableQuerySegment<DynamicTableEntity> queryResult)
        {
            m_continuationToken = queryResult.ContinuationToken;
            m_executionStarted = true;
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

        private List<Dictionary<string, object>> AllocateResult()
        {
            return m_take.HasValue 
                ? new List<Dictionary<string, object>>(m_take.Value) 
                : new List<Dictionary<string, object>>();
        }

        private TableQuery<DynamicTableEntity> PrepareQuery(string filter)
        {
            var query = filter.IsNotNullOrEmpty()
                ? new TableQuery<DynamicTableEntity>().Select(m_properties).Where(filter)
                : new TableQuery<DynamicTableEntity>().Select(m_properties);

            if (m_take.HasValue)
            {
                query = query.Take(m_take.Value);
            }

            return query;
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
