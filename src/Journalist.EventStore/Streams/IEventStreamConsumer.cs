using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumer
    {
        Task<ReceivingResultCode> ReceiveEventsAsync();

        Task CommitProcessedStreamVersionAsync(bool skipCurrent);

        Task CloseAsync();

        IEnumerable<JournaledEvent> EnumerateEvents();

        string StreamName { get; }
    }
}
