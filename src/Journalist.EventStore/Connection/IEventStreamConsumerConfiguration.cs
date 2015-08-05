using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public interface IEventStreamConsumerConfiguration
    {
        IEventStreamConsumerConfiguration ReadStream(string streamName);

        IEventStreamConsumerConfiguration UseConsumerName(string consumerName);

        IEventStreamConsumerConfiguration UseConsumerId(EventStreamConsumerId consumerId);

        IEventStreamConsumerConfiguration AutoCommitProcessedStreamPosition(bool autoCommit);
    }
}
