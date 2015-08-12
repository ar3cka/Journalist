using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSessionFactory
    {
        IEventStreamConsumingSession CreateSession(EventStreamReaderId consumerId, string streamName);
    }
}
