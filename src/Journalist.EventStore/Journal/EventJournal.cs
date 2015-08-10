using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal
{
    public class EventJournal : IEventJournal
    {
        private readonly ICloudTable m_table;

        public EventJournal(ICloudTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public async Task<EventStreamPosition> AppendEventsAsync(
            string streamName,
            EventStreamPosition position,
            IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(events, "events");

            var batch = m_table.PrepareBatchOperation();

            var targetVersion = position.Version.Increment(events.Count);
            WriteHeadProperty(streamName, position, (int)targetVersion, batch);
            WriteEvents(streamName, position.Version, events, batch);

            try
            {
                var batchResult = await batch.ExecuteAsync();
                var headResult = batchResult[0];

                return new EventStreamPosition(headResult.ETag, targetVersion);
            }
            catch (BatchOperationException exception)
            {
                if (exception.OperationBatchNumber == 0 && IsConcurrencyException(exception))
                {
                    throw new EventStreamConcurrencyException(
                        "Event stream '{0}' was concurrently updated.".FormatString(streamName),
                        exception);
                }

                throw;
            }
        }

        public async Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var position = await ReadEndOfStreamPositionAsync(streamName);
            if (EventStreamPosition.IsNewStream(position))
            {
                return EventStreamCursor.Empty;
            }

            return new EventStreamCursor(
                position,
                StreamVersion.Start,
                from => FetchEvents(streamName, from, position.Version, sliceSize));
        }

        public async Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, StreamVersion fromVersion, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var position = await ReadEndOfStreamPositionAsync(streamName);
            if (position.Version < fromVersion)
            {
                return EventStreamCursor.Empty;
            }

            return new EventStreamCursor(
                position,
                fromVersion,
                from => FetchEvents(streamName, from, position.Version, sliceSize));
        }

        public async Task<EventStreamPosition> ReadEndOfStreamPositionAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var headProperties = await ReadHeadAsync(streamName);
            if (headProperties == null)
            {
                return EventStreamPosition.Unknown;
            }

            var timestamp = (string)headProperties[EventJournalTableRowPropertyNames.ETag];
            var version = StreamVersion.Create((int)headProperties[EventJournalTableRowPropertyNames.Version]);

            return new EventStreamPosition(timestamp, version);
        }

        public async Task<StreamVersion> ReadStreamReaderPositionAsync(string streamName, string readerName)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(readerName, "readerName");

            var referenceRow = await ReadReferenceRowHeadAsync(
                streamName,
                "RDR_" + readerName);

            if (referenceRow == null)
            {
                return StreamVersion.Unknown;
            }

            return StreamVersion.Create((int)referenceRow[EventJournalTableRowPropertyNames.Version]);
        }

        public async Task CommitStreamReaderPositionAsync(
            string streamName,
            string readerName,
            StreamVersion readerVersion)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(readerName, "readerName");

            var referenceRow = await ReadReferenceRowHeadAsync(
                streamName,
                "RDR_" + readerName);

            var operation = m_table.PrepareBatchOperation();
            if (referenceRow == null)
            {
                operation.Insert(
                    streamName,
                    "RDR_" + readerName,
                    new Dictionary<string, object>
                    {
                        { EventJournalTableRowPropertyNames.Version, (int)readerVersion }
                    });
            }
            else
            {
                operation.Merge(
                    streamName,
                    "RDR_" + readerName,
                    (string)referenceRow[KnownProperties.ETag],
                    new Dictionary<string, object>
                    {
                        { EventJournalTableRowPropertyNames.Version, (int)readerVersion }
                    });
            }

            await operation.ExecuteAsync();
        }

        private Task<IDictionary<string, object>> ReadHeadAsync(string streamName)
        {
            return ReadReferenceRowHeadAsync(streamName, "HEAD");
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

        private static void WriteHeadProperty(string stream, EventStreamPosition position, int targetVersion, IBatchOperation batch)
        {
            var headProperties = new Dictionary<string, object>
            {
                {EventJournalTableRowPropertyNames.Version, targetVersion}
            };

            if (EventStreamPosition.IsNewStream(position))
            {
                batch.Insert(stream, "HEAD", headProperties);
            }
            else
            {
                batch.Merge(stream, "HEAD", position.ETag, headProperties);
            }
        }

        private async Task<FetchEventsResult> FetchEvents(
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
            var streamPosition = EventStreamPosition.Unknown;
            foreach (var properties in queryResult)
            {
                var rowKey = (string)properties[KnownProperties.RowKey];
                if (rowKey.EqualsCi("HEAD"))
                {
                    streamPosition = new EventStreamPosition(
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
            return exception.HttpStatusCode == HttpStatusCode.Conflict // Inserting twice HEAD record.
                   || exception.HttpStatusCode == HttpStatusCode.PreconditionFailed;
                // Stream concurrent update occured. Head ETag header was changed.
        }
    }
}
