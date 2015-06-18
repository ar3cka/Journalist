using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamProducer
    {
        Task PublishAsync(IReadOnlyCollection<JournaledEvent> events);
    }
}