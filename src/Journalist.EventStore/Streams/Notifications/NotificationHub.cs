using System.Threading.Tasks;
using Journalist.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public class NotificationHub : INotificationHub
    {
        public Task NotifyAsync(EventStreamUpdated eventStream)
        {
            return TaskDone.Done;
        }
    }
}
