using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public interface IEventStreamConsumerConfiguration
    {
        IEventStreamConsumerConfiguration ReadStream(string streamName, bool startReadingFromEnd = false);

        IEventStreamConsumerConfiguration UseConsumerName(string consumerName);

        IEventStreamConsumerConfiguration UseConsumerId(EventStreamReaderId consumerId);

        IEventStreamConsumerConfiguration AutoCommitProcessedStreamPosition(bool autoCommit);
    }
}
