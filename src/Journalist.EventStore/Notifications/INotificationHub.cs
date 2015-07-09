using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Streams;

namespace Journalist.EventStore.Notifications
{
    public interface INotificationHub
    {
        Task NotifyAsync(EventStreamUpdated notification);
    }
}
