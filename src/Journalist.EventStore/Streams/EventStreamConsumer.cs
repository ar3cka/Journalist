using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumer : IEventStreamConsumer
    {
        private readonly Func<StreamVersion, Task<IEventStreamReader>> m_createReader;
        private IEventStreamReader m_reader;
        private bool m_hasUnprocessedEvents;
        private bool m_consuming;
        private StreamVersion m_consumedVersion;
        private bool m_readerExhausted;

        public EventStreamConsumer(
            StreamVersion initialStreamVersion,
            Func<StreamVersion, Task<IEventStreamReader>> createReader)
        {
            Require.NotNull(createReader, "createReader");

            m_consumedVersion = initialStreamVersion;
            m_createReader = createReader;
            m_readerExhausted = true;
        }

        public async Task<bool> ReceiveEventsAsync()
        {
            if (m_readerExhausted)
            {
                m_reader = await m_createReader(m_consumedVersion.Increment(1));
                m_readerExhausted = false;
            }

            if (m_reader.HasEvents)
            {
                await m_reader.ReadEventsAsync();
                m_hasUnprocessedEvents = true;

                return true;
            }

            return false;
        }

        public Task RememberConsumedStreamVersionAsync()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
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
                    yield return m_reader.Events[eventSliceOffset];
                }

                m_consuming = false;
                m_hasUnprocessedEvents = false;
                m_consumedVersion = m_reader.CurrentStreamVersion;

                if (!m_reader.HasEvents)
                {
                    m_readerExhausted = true;
                }

                yield break;
            }

            throw new InvalidOperationException("Consumer stream is empty.");
        }
    }
}
