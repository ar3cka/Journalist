using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamWriter
    {
        int StreamPosition { get; }

        Task AppendEvents(IReadOnlyCollection<JournaledEvent> events);
    }
}
