using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public interface IEventStreamConsumerConfiguration
    {
        IEventStreamConsumerConfiguration ReadStream(string streamName, bool startReadingFromEnd = false);

        IEventStreamConsumerConfiguration UseConsumerName(string consumerName);

        IEventStreamConsumerConfiguration UseConsumerId(EventStreamConsumerId consumerId);

        IEventStreamConsumerConfiguration AutoCommitProcessedStreamPosition(bool autoCommit);
    }
}
