using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSessionFactory
    {
        IEventStreamConsumingSession CreateSession(EventStreamReaderId readerId, string streamName);
    }
}
