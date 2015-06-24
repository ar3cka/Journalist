using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumingSession
    {
        Task<bool> PromoteToLeaderAsync(string consumerId);

        Task FreeAsync();

        string StreamName { get; }
    }
}
