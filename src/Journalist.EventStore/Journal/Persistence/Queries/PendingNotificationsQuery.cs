using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence.Queries
{
    public class PendingNotificationsQuery : IStreamQuery<EventStreamUpdated>
    {
        private const int MAX_ROWS_COUNT = 1000;

        private static readonly string s_queryTemplate = "RowKey ge '{0}{1}' and RowKey le '{0}{2}'".FormatString(
            EventJournalTableKeys.PendingNotificationPrefix,
            StreamVersion.Unknown,
            StreamVersion.Max);

        private static readonly string[] s_separators = { "|" };

        private readonly ICloudTable m_table;
        private ICloudTableEntitySegmentedRangeQuery m_query;

        public PendingNotificationsQuery(ICloudTable table)
        {
            m_table = table;
        }

        public void Prepare()
        {
            m_query = m_table.PrepareEntityFilterSegmentedRangeQuery(s_queryTemplate);
        }

        public async Task<IReadOnlyList<EventStreamUpdated>> ExecuteAsync()
        {
            AssertQueryPrepared();

            var result = new List<EventStreamUpdated>();
            do
            {
                var queryResult = await m_query.ExecuteAsync();
                if (queryResult != null)
                {
                    foreach (var row in queryResult)
                    {
                        var streamName = (string)row[KnownProperties.PartitionKey];
                        var fromVersion = ((string)row[KnownProperties.RowKey]).Split(s_separators, StringSplitOptions.RemoveEmptyEntries)[1];
                        var toVersion = (int)row[EventJournalTableRowPropertyNames.Version];

                        result.Add(new EventStreamUpdated(
                            streamName,
                            StreamVersion.Parse(fromVersion),
                            StreamVersion.Create(toVersion)));
                    }
                }
            }
            while (m_query.HasMore || result.Count < MAX_ROWS_COUNT);

            return result;
        }

        private void AssertQueryPrepared()
        {
            Ensure.True(m_query != null, "m_query != null");
        }
    }
}