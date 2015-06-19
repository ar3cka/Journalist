using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public class EventStreamProducer : IEventStreamProducer
    {
        private readonly IEventStreamWriter m_streamWriter;

        public EventStreamProducer(IEventStreamWriter streamWriter)
        {
            Require.NotNull(streamWriter, "streamWriter");

            m_streamWriter = streamWriter;
        }

        public async Task PublishAsync(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            try
            {
                await m_streamWriter.AppendEventsAsync(events);
            }
            catch (EventStreamConcurrencyException)
            {
            }

            await m_streamWriter.MoveToEndOfStreamAsync();
            await m_streamWriter.AppendEventsAsync(events);
        }
    }
}
