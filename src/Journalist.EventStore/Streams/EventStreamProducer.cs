using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

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

        public Task PublishAsync(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            return m_streamWriter.AppendEvents(events);
        }
    }
}