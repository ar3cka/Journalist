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
        private int m_uncommittedEventsCount = 0;
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
                m_uncommittedEventsCount = 0;
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

        public async Task RememberConsumedEventsAsync()
        {
            if (m_consuming)
            {
                var version = m_commitedStreamVersion.Increment(m_uncommittedEventsCount);
                await m_commitConsumedVersion(version);

                m_commitedStreamVersion = version;
                m_uncommittedEventsCount = 0;
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

                for (var eventSliceOffset = 0; eventSliceOffset < m_reader.Events.Count; eventSliceOffset++)
                {
                    m_uncommittedEventsCount++;

                    yield return m_reader.Events[eventSliceOffset];
                }

                m_consuming = false;
                m_hasUnprocessedEvents = false;

                yield break;
            }

            throw new InvalidOperationException("Consumer stream is empty.");
        }
    }
}
