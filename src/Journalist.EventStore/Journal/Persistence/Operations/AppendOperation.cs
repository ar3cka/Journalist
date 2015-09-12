using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence.Operations
{
    public class AppendOperation : IStreamOperation<EventStreamHeader>
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

            IncrementStreamVersion(events);
            UpdateHead();
            AppendEvents(events);
            AppendUnpublishedVersionRecord();
        }

        public async Task<EventStreamHeader> ExecuteAsync()
        {
            Ensure.True(m_operation != null, "Operation was not prepared.");

            var batchResult = await m_operation.ExecuteAsync();

            return new EventStreamHeader(
                batchResult[0].ETag,
                m_targetVersion);
        }

        public void Handle(Exception exception)
        {
            var batchOperationException = exception as BatchOperationException;
            if (batchOperationException != null)
            {
                if (batchOperationException.OperationBatchNumber == 0 &&
                    IsConcurrencyException(batchOperationException))
                {
                    throw new EventStreamConcurrencyException(
                        "Event stream '{0}' was concurrently updated.".FormatString(m_streamName),
                        exception);
                }
            }
        }

        private void IncrementStreamVersion(IReadOnlyCollection<JournaledEvent> events)
        {
            m_targetVersion = m_header.Version.Increment(events.Count);
        }

        private void UpdateHead()
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

        private void AppendEvents(IEnumerable<JournaledEvent> events)
        {
            var currentVersion = m_header.Version;
            foreach (var journaledEvent in events)
            {
                currentVersion = currentVersion.Increment(1);

                m_operation.Insert(
                    m_streamName,
                    currentVersion.ToString(),
                    journaledEvent.ToDictionary());
            }
        }

        private void AppendUnpublishedVersionRecord()
        {
           m_operation.Insert(m_streamName, "PNDNTF|" + m_targetVersion);
        }

        private static bool IsConcurrencyException(BatchOperationException exception)
        {
            return exception.HttpStatusCode == HttpStatusCode.Conflict ||         // Inserting twice HEAD record.
                   exception.HttpStatusCode == HttpStatusCode.PreconditionFailed; // Stream concurrent update occured. Head ETag header was changed.
        }
    }
}
