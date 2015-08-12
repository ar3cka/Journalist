using System.Threading.Tasks;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumersRegistry
    {
        Task<EventStreamReaderId> RegisterAsync(string consumerName);
    }
}
