using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.Options;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumers
    {
        Task<EventStreamReaderId> RegisterAsync(string consumerName);

	    Task<Option<string>> TryGetNameAsync(EventStreamReaderId eventStreamReaderId);
    }
}
