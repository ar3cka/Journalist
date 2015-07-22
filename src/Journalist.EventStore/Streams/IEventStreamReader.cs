using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamReader : IEventStreamInteractionEntity
    {
        Task ReadEventsAsync();

        Task ContinueAsync();

        IReadOnlyList<JournaledEvent> Events { get; }

        bool HasEvents { get; }

        bool IsInitial { get; }

        bool IsReading { get; }

        bool IsCompleted { get; }
    }
}
