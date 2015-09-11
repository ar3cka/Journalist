using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamInteractionEntity
    {
        string StreamName { get; }

        StreamVersion StreamVersion { get; }

        EventStreamHeader StreamHeader { get; }

        bool IsClosed { get; }
    }
}
