using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumer : IEventStreamConsumer
    {
        private readonly IEventStreamReader m_reader;
        private readonly Func<StreamVersion, Task> m_commitConsumedVersion;
        private bool m_hasUnprocessedEvents;
        private bool m_consuming;
        private bool m_receiving;
        private int m_eventSliceOffset;
        private int m_commitedEventCount;
        private StreamVersion m_commitedStreamVersion;

        public EventStreamConsumer(
            IEventStreamReader streamReader,
            StreamVersion commitedStreamVersion,
            Func<StreamVersion, Task> commitConsumedVersion)
        {
            Require.NotNull(streamReader, "streamReader");
            Require.NotNull(commitConsumedVersion, "commitConsumedVersion");

            m_reader = streamReader;
            m_commitConsumedVersion = commitConsumedVersion;
            m_commitedStreamVersion = commitedStreamVersion;
        }

        public async Task<bool> ReceiveEventsAsync()
        {
            if (m_receiving && m_commitedStreamVersion != m_reader.CurrentStreamVersion)
            {
                await m_commitConsumedVersion(m_reader.CurrentStreamVersion);
                m_commitedStreamVersion = m_reader.CurrentStreamVersion;
                m_eventSliceOffset = 0;
            }

            if (m_reader.IsCompleted)
            {
                await m_reader.ContinueAsync();
                m_receiving = false;
            }

            if (m_reader.HasEvents)
            {

                await m_reader.ReadEventsAsync();

                m_hasUnprocessedEvents = true;
                m_receiving = true;

                return true;
            }

            m_receiving = false;

            return false;
        }

        public async Task RememberConsumedEventsAsync(bool skipCurrent)
        {
            if (m_consuming)
            {
                var eventNumber = skipCurrent ? m_eventSliceOffset : m_eventSliceOffset + 1;
                var version = m_commitedStreamVersion.Increment(eventNumber - m_commitedEventCount);

                if (m_commitedStreamVersion < version)
                {
                    await m_commitConsumedVersion(version);

                    m_commitedStreamVersion = version;
                    m_commitedEventCount++;
                }

                return;
            }

            throw new InvalidOperationException("Stream is not opened.");
        }

        public async Task CloseAsync()
        {
            if (m_hasUnprocessedEvents)
            {
                throw new InvalidOperationException("Stream has unhandled events.");
            }

            if (m_receiving)
            {
                await m_commitConsumedVersion(m_reader.CurrentStreamVersion);
                m_commitedStreamVersion = m_reader.CurrentStreamVersion;

                m_receiving = false;
            }
        }

        public IEnumerable<JournaledEvent> EnumerateEvents()
        {
            if (m_consuming)
            {
                throw new InvalidOperationException("Consumer stream is already opened.");
            }

            if (m_hasUnprocessedEvents)
            {
                m_consuming = true;

                for (m_eventSliceOffset = 0; m_eventSliceOffset < m_reader.Events.Count; m_eventSliceOffset++)
                {
                    yield return m_reader.Events[m_eventSliceOffset];
                }

                m_consuming = false;
                m_hasUnprocessedEvents = false;

                yield break;
            }

            throw new InvalidOperationException("Consumer stream is empty.");
        }
    }
}
