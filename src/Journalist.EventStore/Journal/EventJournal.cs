﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.EventStore.Streams;
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

        public Task<EventStreamHeader> AppendEventsAsync(
            string streamName,
            EventStreamHeader header,
            IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(events, "events");

            var operation = m_table.CreateAppendOperation(streamName, header);
            operation.Prepare(events);

            return ExecuteOperationAsync(operation);
        }

        public async Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var header = await ReadStreamHeaderAsync(streamName);
            if (EventStreamHeader.IsNewStream(header))
            {
                return EventStreamCursor.UninitializedStream;
            }

            return EventStreamCursor.CreateActiveCursor(
                header,
                StreamVersion.Start,
                from => m_table.FetchStreamEvents(streamName, header, from, sliceSize));
        }

        public async Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, StreamVersion fromVersion, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var header = await ReadStreamHeaderAsync(streamName);
            if (header.Version == StreamVersion.Unknown)
            {
                return EventStreamCursor.UninitializedStream;
            }

            if (header.Version < fromVersion)
            {
                return EventStreamCursor.CreateEmptyCursor(header, fromVersion);
            }

            return EventStreamCursor.CreateActiveCursor(
                header,
                fromVersion,
                from => m_table.FetchStreamEvents(streamName, header, from, sliceSize));
        }

        public async Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, EventStreamReaderId readerId, int sliceSize)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(sliceSize, "sliceSize");

            var fromVersion = await ReadStreamReaderPositionAsync(streamName, readerId);
            var header = await ReadStreamHeaderAsync(streamName);

            if (header == EventStreamHeader.Unknown)
            {
                return EventStreamCursor.UninitializedStream;
            }

            if (header.Version <= fromVersion)
            {
                return EventStreamCursor.CreateEmptyCursor(header, fromVersion);
            }

            return EventStreamCursor.CreateActiveCursor(
                header,
                fromVersion.Increment(),
                from => m_table.FetchStreamEvents(streamName, header, from, sliceSize));
        }

        public async Task<EventStreamHeader> ReadStreamHeaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var headProperties = await m_table.ReadStreamHeadPropertiesAsync(streamName);
            if (headProperties == null)
            {
                return EventStreamHeader.Unknown;
            }

            var timestamp = (string)headProperties[EventJournalTableRowPropertyNames.ETag];
            var version = StreamVersion.Create((int)headProperties[EventJournalTableRowPropertyNames.Version]);

            return new EventStreamHeader(timestamp, version);
        }

        public async Task<StreamVersion> ReadStreamReaderPositionAsync(string streamName, EventStreamReaderId readerId)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(readerId, "readerId");

            var properties = await m_table.ReadStreamReaderPropertiesAsync(streamName, readerId);
            if (properties == null)
            {
                return StreamVersion.Unknown;
            }

            return StreamVersion.Create((int)properties[EventJournalTableRowPropertyNames.Version]);
		}

		public async Task<IEnumerable<StreamReaderDescription>> GetStreamReadersDescriptionsAsync(string streamName)
		{
			Require.NotEmpty(streamName, nameof(streamName));

			var streamReadersProperties = await m_table.ReadAllStreamReadersPropertiesAsync(streamName);

			var descriptions = streamReadersProperties.Select(streamReaderProperties => new StreamReaderDescription(
				streamName,
				EventStreamReaderId.Parse(streamReaderProperties[KnownProperties.RowKey].ToString().Split('|').Last()),
				StreamVersion.Create((int)streamReaderProperties[EventJournalTableRowPropertyNames.Version])));

			return descriptions;
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
                await m_table.InsertStreamReaderPropertiesAsync(
                    streamName,
                    readerId,
                    readerVersion);
            }
            else
            {
                var readerVersionValue = (int)readerVersion;
                var savedVersionValue = (int)properties[EventJournalTableRowPropertyNames.Version];
                var etag = (string)properties[KnownProperties.ETag];
                Ensure.True(savedVersionValue <= readerVersionValue,
                    "Saved reader stream version is greater then passed value.");

                if (readerVersionValue != savedVersionValue)
                {
                    await m_table.UpdateStreamReaderPropertiesAsync(streamName, readerId, readerVersion, etag);
                }
            }
        }

        private static async Task<TResult> ExecuteOperationAsync<TResult>(IStreamOperation<TResult> operation)
        {
            try
            {
                return await operation.ExecuteAsync();
            }
            catch (Exception exception)
            {
                operation.Handle(exception);

                throw;
            }
        }
    }
}
