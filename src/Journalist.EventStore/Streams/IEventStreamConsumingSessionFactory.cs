namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSessionFactory
    {
        IEventStreamConsumingSession CreateSession(string streamName);
    }
}