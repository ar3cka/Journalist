using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal
{
    public class EventJournal : IEventJournal
    {
        private readonly IEventJournalTable m_table;

        public EventJournal(IEventJournalTable table)
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

            var targetVersion = position.Version.Increment(events.Count);
            try
            {
                var result = await m_table.InsertEventsAsync(
                    streamName,
                    position,
                    events);

                return new EventStreamPosition(result.ETag, targetVersion);
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
                from => m_table.FetchStreamEvents(streamName, from, position.Version, sliceSize));
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
                from => m_table.FetchStreamEvents(streamName, from, position.Version, sliceSize));
        }

        public async Task<EventStreamPosition> ReadEndOfStreamPositionAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var headProperties = await m_table.ReadStreamHeadPropertiesAsync(streamName);
            if (headProperties == null)
            {
                return EventStreamPosition.Unknown;
            }

            var timestamp = (string)headProperties[EventJournalTableRowPropertyNames.ETag];
            var version = StreamVersion.Create((int)headProperties[EventJournalTableRowPropertyNames.Version]);

            return new EventStreamPosition(timestamp, version);
        }

        public async Task<StreamVersion> ReadStreamReaderPositionAsync(string streamName, EventStreamReaderId readerId)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(readerId, "readerId");

            var referenceRow = await m_table.ReadStreamReaderPropertiesAsync(streamName, readerId);
            if (referenceRow == null)
            {
                return StreamVersion.Unknown;
            }

            return StreamVersion.Create((int)referenceRow[EventJournalTableRowPropertyNames.Version]);
        }

        public async Task CommitStreamReaderPositionAsync(
            string streamName,
            EventStreamReaderId readerId,
            StreamVersion readerVersion)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(readerId, "readerId");

            var properties = await m_table.ReadStreamReaderPropertiesAsync(streamName, readerId);
            if (properties == null)
            {
                await m_table.InserStreamReaderPropertiesAsync(
                    streamName,
                    readerId,
                    readerVersion);
            }
            else
            {
                await m_table.UpdateStreamReaderPropertiesAsync(
                    streamName,
                    readerId,
                    readerVersion,
                    (string)properties[KnownProperties.ETag]);
            }
        }

        private static bool IsConcurrencyException(BatchOperationException exception)
        {
            return exception.HttpStatusCode == HttpStatusCode.Conflict ||         // Inserting twice HEAD record.
                   exception.HttpStatusCode == HttpStatusCode.PreconditionFailed; // Stream concurrent update occured. Head ETag header was changed.
        }
    }
}
