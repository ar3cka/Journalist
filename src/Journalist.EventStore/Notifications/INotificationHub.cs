using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications
{
    public interface INotificationHub
    {
        Task NotifyAsync(EventStreamUpdated notification);
    }
}
