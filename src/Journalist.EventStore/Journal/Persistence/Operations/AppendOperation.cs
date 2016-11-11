using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence.Operations
{
    public class AppendOperation : JournalTableOperation<EventStreamHeader>
    {
        private readonly EventStreamHeader m_header;
        private StreamVersion m_targetVersion;

        public AppendOperation(ICloudTable table, string streamName, EventStreamHeader header)
            : base(table, streamName)
        {
            m_header = header;
        }

        public void Prepare(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            PrepareBatchOperation();
            IncrementStreamVersion(events);
            UpdateHead();
            AppendEvents(events);
        }

        public override async Task<EventStreamHeader> ExecuteAsync()
        {
            var batchResult = await ExecuteBatchOperationAsync();

            return new EventStreamHeader(batchResult[0].ETag, m_targetVersion);
        }

        public override void Handle(Exception exception)
        {
            var batchOperationException = exception as BatchOperationException;
            if (batchOperationException != null)
            {
                if (batchOperationException.OperationBatchNumber == 0 && IsConcurrencyException(batchOperationException))
                {
                    throw new EventStreamConcurrencyException(
                        "Event stream '{0}' was concurrently updated.".FormatString(StreamName),
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
            if (EventStreamHeader.IsNewStream(m_header))
            {
                Insert(EventJournalTableKeys.Header, EventJournalTableRowPropertyNames.Version, (int)m_targetVersion);
            }
            else
            {
                Merge(EventJournalTableKeys.Header, m_header.ETag, EventJournalTableRowPropertyNames.Version, (int)m_targetVersion);
            }
        }

        private void AppendEvents(IEnumerable<JournaledEvent> events)
        {
            var currentVersion = m_header.Version;
            foreach (var journaledEvent in events)
            {
                currentVersion = currentVersion.Increment(1);

                Insert(currentVersion.ToString(), journaledEvent.ToDictionary());
            }
        }

        private void InsertPendingNotification()
        {
            var rowKey = EventJournalTableKeys.GetPendingNotificationRowKey(m_header.Version);

            Insert(rowKey, EventJournalTableRowPropertyNames.Version, (int)m_targetVersion);
        }

        private static bool IsConcurrencyException(BatchOperationException exception)
        {
            return exception.HttpStatusCode == HttpStatusCode.Conflict ||         // Inserting twice HEAD record.
                   exception.HttpStatusCode == HttpStatusCode.PreconditionFailed; // Stream concurrent update occured. Head ETag header was changed.
        }
    }
}
