using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamReader : IEventStreamInteractionEntity
    {
        Task ReadEventsAsync();

        StreamVersion ReaderStreamVersion { get; }

        IReadOnlyList<JournaledEvent> Events { get; }

        bool HasEvents { get; }
    }
}
