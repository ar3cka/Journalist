using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications
{
    public interface INotificationHub
    {
        Task NotifyAsync(INotification notification);
    }
}
