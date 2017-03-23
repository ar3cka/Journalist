using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Listeners;

namespace Journalist.EventStore.Notifications
{
	public interface IFailedNotificationsHub
	{
		Task PutToFailedAsync(INotification notification);

		Task<INotification[]> ReceiveFailedAsync();

		Task<bool> RetryNotificationAsync(NotificationId notificationId);

		Task RemoveFromFailedAsync(NotificationId[] notificationIds);

		void AddRetryListener(INotificationListener notificationsListener);
	}
}