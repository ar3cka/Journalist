using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumerStateMachine
    {
        void ReceivingStarted();

        void ReceivingCompleted(int eventsCount);

        void ConsumingCompleted();

        void ConsumingStarted();

        void ConsumerClosed();

        void EventProcessingStarted();

        bool CommitRequired(bool autoCommitProcessedStreamVersion);

        StreamVersion CalculateConsumedStreamVersion(bool skipCurrentEvent);

        void ConsumedStreamVersionCommited(StreamVersion version, bool skipCurrent);

        StreamVersion CommitedStreamVersion { get; }
    }
}
