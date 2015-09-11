using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamInteractionEntity
    {
        string StreamName { get; }

        StreamVersion StreamVersion { get; }

        EventStreamPosition StreamPosition { get; }

        bool IsClosed { get; }
    }
}
