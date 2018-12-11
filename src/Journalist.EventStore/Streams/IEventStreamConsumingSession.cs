using System.Threading.Tasks;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSession
    {
        Task<bool> PromoteToLeaderAsync();

        Task FreeAsync();

        EventStreamReaderId ConsumerId { get; }
    }
}
