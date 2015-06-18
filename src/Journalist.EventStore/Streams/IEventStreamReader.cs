using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamReader
    {
        Task ReadEventsAsync();

        IReadOnlyList<JournaledEvent> Events { get; }

        bool HasMoreEvents { get; }

        string StreamName { get; }
    }
}
