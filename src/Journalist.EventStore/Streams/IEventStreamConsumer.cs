using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumer
    {
        Task<bool> ReceiveEventsAsync();

        Task RememberConsumedEventsAsync();

        Task CloseAsync();

        IEnumerable<JournaledEvent> EnumerateEvents();
    }
}
