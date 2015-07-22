namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSessionFactory
    {
        IEventStreamConsumingSession CreateSession(EventStreamConsumerId consumerId, string streamName);
    }
}
