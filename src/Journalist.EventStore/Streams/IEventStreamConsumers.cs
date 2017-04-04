using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumers
    {
        Task<EventStreamReaderId> RegisterAsync(string consumerName);

        Task<IEnumerable<EventStreamReaderId>> EnumerateAsync();
    }
}
