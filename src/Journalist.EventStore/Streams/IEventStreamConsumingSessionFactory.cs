namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSessionFactory
    {
        IEventStreamConsumingSession CreateSession(string consumerName, string streamName);
    }
}
