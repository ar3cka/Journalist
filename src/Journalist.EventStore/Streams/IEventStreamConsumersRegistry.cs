using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumersRegistry
    {
        Task<EventStreamConsumerId> RegisterAsync(string consumerName);

        Task<bool> IsResistedAsync(EventStreamConsumerId consumerId);
    }
}
