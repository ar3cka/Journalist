using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Persistence
{
    public interface IFailedNotifications
    {
        Task AddAsync(IFailedNotification failedNotification);

        Task DeleteAsync(string failedNotificationId);
    }
}
