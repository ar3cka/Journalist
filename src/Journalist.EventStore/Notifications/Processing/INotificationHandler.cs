using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Processing
{
    public interface INotificationHandler
    {
        Task HandleNotificationAsync(INotification notification);
    }
}
