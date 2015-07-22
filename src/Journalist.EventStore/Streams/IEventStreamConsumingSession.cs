using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSession
    {
        Task<bool> PromoteToLeaderAsync();

        Task FreeAsync();

        EventStreamConsumerId ConsumerId { get; }
    }
}
