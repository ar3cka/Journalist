using System.Threading.Tasks;
using Journalist.Tasks;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumingSession : IEventStreamConsumingSession
    {
        public Task<bool> PromoteToLeaderAsync(string consumerId)
        {
            return TaskDone.True;
        }

        public Task FreeAsync(string consumerId)
        {
            return TaskDone.Done;
        }
    }
}
