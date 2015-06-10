using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal
{
    public class EventJournal : IEventJournal
    {
        private readonly ICloudTable m_table;
        private const int DEFAULT_SLICE_SIZE = 1000;

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
            WriteHeadProperty(streamName, position, (int) targetVersion, batch);
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

        public async Task<EventStreamCursor> OpenEventStreamAsync(string streamName, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var position = await ReadEndOfStreamPositionAsync(streamName);
            if (EventStreamPosition.IsAtStart(position))
            {
                return EventStreamCursor.Empty;
            }

            return new EventStreamCursor(
                position,
                StreamVersion.Zero,
                from => FetchEvents(streamName, from, position.Version, sliceSize));
        }

        public Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            return OpenEventStreamAsync(streamName, fromVersion, DEFAULT_SLICE_SIZE);
        }

        public async Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion,
            int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var position = await ReadEndOfStreamPositionAsync(streamName);
            return new EventStreamCursor(
                position,
                fromVersion,
                from => FetchEvents(streamName, from, position.Version, sliceSize));
        }

        public Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion,
            StreamVersion toVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            return OpenEventStreamAsync(streamName, fromVersion, toVersion, DEFAULT_SLICE_SIZE);
        }

        public async Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion,
            StreamVersion toVersion, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var headProperties = await ReadHeadAsync(streamName);
            if (headProperties == null)
            {
                return EventStreamCursor.Empty;
            }

            return new EventStreamCursor(
                new EventStreamPosition(string.Empty, toVersion),
                fromVersion,
                from => FetchEvents(streamName, from, toVersion, sliceSize));
        }

        public Task<EventStreamCursor> OpenEventStreamAsync(string streamName)
        {
            return OpenEventStreamAsync(streamName, DEFAULT_SLICE_SIZE);
        }

        public async Task<EventStreamPosition> ReadEndOfStreamPositionAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var headProperties = await ReadHeadAsync(streamName);
            if (headProperties == null)
            {
                return EventStreamPosition.Start;
            }

            var timestamp = (string) headProperties[EventJournalTableRowPropertyNames.ETag];
            var version = StreamVersion.Create((int) headProperties[EventJournalTableRowPropertyNames.Version]);

            return new EventStreamPosition(timestamp, version);
        }

        private async Task<IDictionary<string, object>> ReadHeadAsync(string streamName)
        {
            var query = m_table.PrepareEntityPointQuery(streamName, "HEAD",
                EventJournalTableRowPropertyNames.Version.YieldArray());
            var headProperties = await query.ExecuteAsync();

            return headProperties;
        }

        private static void WriteHeadProperty(string stream, EventStreamPosition position, int targetVersion,
            IBatchOperation batch)
        {
            var headProperties = new Dictionary<string, object>
            {
                {EventJournalTableRowPropertyNames.Version, targetVersion}
            };

            if (EventStreamPosition.IsAtStart(position))
            {
                batch.Insert(stream, "HEAD", headProperties);
            }
            else
            {
                batch.Merge(stream, "HEAD", position.ETag, headProperties);
            }
        }

        private async Task<SortedList<StreamVersion, JournaledEvent>> FetchEvents(string stream,
            StreamVersion fromVersion, StreamVersion toVersion, int sliceSize)
        {
            var nextSliceVersion = fromVersion.Increment(sliceSize);
            if (nextSliceVersion >= toVersion)
            {
                nextSliceVersion = toVersion;
            }

            var query = m_table.PrepareEntityFilterRangeQuery(
                "(PartitionKey eq '{0}') and (RowKey ge '{1}' and RowKey le '{2}')".FormatString(
                    stream,
                    fromVersion.ToString(),
                    nextSliceVersion.ToString()), JournaledEventPropertyNames.All);

            var queryResult = await query.ExecuteAsync();

            var result = new SortedList<StreamVersion, JournaledEvent>(sliceSize);
            foreach (var properties in queryResult)
            {
                result.Add(StreamVersion.Parse((string)properties[KnownProperties.RowKey]), JournaledEvent.Create(properties));
            }

            return result;
        }

        private static void WriteEvents(string stream, StreamVersion version, IEnumerable<JournaledEvent> events,
            IBatchOperation batch)
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
