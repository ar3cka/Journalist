namespace Journalist.EventStore.Connection
{
    public interface IEventStreamConsumerConfiguration
    {
        IEventStreamConsumerConfiguration ReadStream(string streamName, bool startReadingFromEnd = false);

        IEventStreamConsumerConfiguration WithName(string consumerName);

        IEventStreamConsumerConfiguration AutoCommitProcessedStreamPosition(bool autoCommit);
    }
}
