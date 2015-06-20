using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumer : IEventStreamConsumer
    {
        private readonly IEventStreamReader m_reader;
        private bool m_hasUnprocessedEvents;
        private bool m_consuming;

        public EventStreamConsumer(IEventStreamReader reader)
        {
            Require.NotNull(reader, "reader");

            m_reader = reader;
        }

        public async Task<bool> ReceiveEventsAsync()
        {
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

                foreach (var journaledEvent in m_reader.Events)
                {
                    yield return journaledEvent;
                }

                m_consuming = false;
                m_hasUnprocessedEvents = false;

                yield break;
            }

            throw new InvalidOperationException("Consumer stream is empty.");
        }
    }
}
