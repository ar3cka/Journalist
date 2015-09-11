using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence.Operations
{
    public class AppendOperation
    {
        private readonly ICloudTable m_table;
        private readonly string m_streamName;
        private readonly EventStreamHeader m_header;

        private IBatchOperation m_operation;
        private StreamVersion m_targetVersion;

        public AppendOperation(ICloudTable table, string streamName, EventStreamHeader header)
        {
            Require.NotNull(table, "table");
            Require.NotEmpty(streamName, "steamName");

            m_table = table;
            m_streamName = streamName;
            m_header = header;
        }

        public void Prepare(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_operation = m_table.PrepareBatchOperation();

            m_targetVersion = m_header.Version.Increment(events.Count);
            WriteHeadProperty();
            WriteEvents(events);
        }

        public async Task<EventStreamHeader> ExecuteAsync()
        {
            Ensure.True(m_operation != null, "Operation was not prepared.");

            var batchResult = await m_operation.ExecuteAsync();

            return new EventStreamHeader(
                batchResult[0].ETag,
                m_targetVersion);
        }

        private void WriteHeadProperty()
        {
            var headProperties = new Dictionary<string, object>
            {
                { EventJournalTableRowPropertyNames.Version, (int)m_targetVersion }
            };

            if (EventStreamHeader.IsNewStream(m_header))
            {
                m_operation.Insert(m_streamName, "HEAD", headProperties);
            }
            else
            {
                m_operation.Merge(m_streamName, "HEAD", m_header.ETag, headProperties);
            }
        }

        private void WriteEvents(IEnumerable<JournaledEvent> events)
        {
            var currentVersion = m_header.Version;
            foreach (var journaledEvent in events)
            {
                currentVersion = currentVersion.Increment(1);

                // InsertOrReplace is faster then Insert operation, because storage engine
                // can skip etag checking.
                m_operation.InsertOrReplace(
                    m_streamName,
                    currentVersion.ToString(),
                    journaledEvent.ToDictionary());
            }
        }
    }
}
