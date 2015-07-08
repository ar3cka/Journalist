using System.Threading.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public interface INotificationHub
    {
        Task NotifyAsync(EventStreamUpdated notification);
    }
}
