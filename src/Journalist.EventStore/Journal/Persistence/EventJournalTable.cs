using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence
{
    public class EventJournalTable : IEventJournalTable
    {
        private readonly ICloudTable m_table;

        public EventJournalTable(ICloudTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public Task<IDictionary<string, object>> ReadStreamHeadPropertiesAsync(string streamName)
        {
            return ReadReferenceRowHeadAsync(streamName, "HEAD");
        }

        public Task<IDictionary<string, object>> ReadStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId)
        {
            return ReadReferenceRowHeadAsync(streamName, "RDR_" + readerId);
        }

        public async Task InserStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version)
        {
            var operation = m_table.PrepareBatchOperation();

            operation.Insert(
                streamName,
                "RDR_" + readerId,
                new Dictionary<string, object>
                {
                    { EventJournalTableRowPropertyNames.Version, (int)version }
                });

            await operation.ExecuteAsync();
        }

        public async Task UpdateStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version, string etag)
        {
            var operation = m_table.PrepareBatchOperation();

            operation.Merge(
                streamName,
                "RDR_" + readerId,
                etag,
                new Dictionary<string, object>
                {
                    { EventJournalTableRowPropertyNames.Version, (int)version }
                });

            await operation.ExecuteAsync();
        }

        public async Task<OperationResult> InsertEventsAsync(
            string streamName,
            EventStreamHeader header,
            IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(events, "events");

            var batch = m_table.PrepareBatchOperation();

            var targetVersion = header.Version.Increment(events.Count);
            WriteHeadProperty(streamName, header, (int)targetVersion, batch);
            WriteEvents(streamName, header.Version, events, batch);

            var batchResult = await batch.ExecuteAsync();
            return batchResult[0];
        }

        public async Task<FetchEventsResult> FetchStreamEvents(
            string stream,
            StreamVersion fromVersion,
            StreamVersion toVersion,
            int sliceSize)
        {
            var nextSliceVersion = fromVersion.Increment(sliceSize);
            if (nextSliceVersion >= toVersion)
            {
                nextSliceVersion = toVersion;
            }

            const string queryTemplate =
                "((PartitionKey eq '{0}') and (RowKey eq 'HEAD')) or " +
                "((PartitionKey eq '{0}') and (RowKey ge '{1}' and RowKey le '{2}'))";

            var query = m_table.PrepareEntityFilterRangeQuery(
                queryTemplate.FormatString(
                    stream,
                    fromVersion.ToString(),
                    nextSliceVersion.ToString()));

            var queryResult = await query.ExecuteAsync();

            var events = new SortedList<StreamVersion, JournaledEvent>(sliceSize);
            var streamPosition = EventStreamHeader.Unknown;
            foreach (var properties in queryResult)
            {
                var rowKey = (string)properties[KnownProperties.RowKey];
                if (rowKey.EqualsCi("HEAD"))
                {
                    streamPosition = new EventStreamHeader(
                        (string)properties[KnownProperties.ETag],
                        StreamVersion.Create((int)properties[EventJournalTableRowPropertyNames.Version]));
                }
                else
                {
                    events.Add(StreamVersion.Parse((string)properties[KnownProperties.RowKey]), JournaledEvent.Create(properties));
                }
            }

            return new FetchEventsResult(streamPosition, events);
        }

        private async Task<IDictionary<string, object>> ReadReferenceRowHeadAsync(string streamName, string referenceType)
        {
            var query = m_table.PrepareEntityPointQuery(
                streamName,
                referenceType,
                EventJournalTableRowPropertyNames.Version.YieldArray());

            var headProperties = await query.ExecuteAsync();

            return headProperties;
        }

        private static void WriteHeadProperty(string stream, EventStreamHeader header, int targetVersion, IBatchOperation batch)
        {
            var headProperties = new Dictionary<string, object>
            {
                {EventJournalTableRowPropertyNames.Version, targetVersion}
            };

            if (EventStreamHeader.IsNewStream(header))
            {
                batch.Insert(stream, "HEAD", headProperties);
            }
            else
            {
                batch.Merge(stream, "HEAD", header.ETag, headProperties);
            }
        }

        private static void WriteEvents(string stream, StreamVersion version, IEnumerable<JournaledEvent> events, IBatchOperation batch)
        {
            var currentVersion = version;
            foreach (var journaledEvent in events)
            {
                currentVersion = currentVersion.Increment(1);

                // InsertOrReplace is faster then Insert operation, because storage engine
                // can skip etag checking.
                batch.InsertOrReplace(stream, currentVersion.ToString(), journaledEvent.ToDictionary());
            }
        }

        private static bool IsConcurrencyException(BatchOperationException exception)
        {
            return exception.HttpStatusCode == HttpStatusCode.Conflict ||         // Inserting twice HEAD record.
                   exception.HttpStatusCode == HttpStatusCode.PreconditionFailed; // Stream concurrent update occured. Head ETag header was changed.
        }
    }
}
