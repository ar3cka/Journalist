using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamWriter : IEventStreamInteractionEntity
    {
        Task AppendEventsAsync(IReadOnlyCollection<JournaledEvent> events);

        Task MoveToEndOfStreamAsync();
    }
}
