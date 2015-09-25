using System;
using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Channels
{
    public interface INotificationsChannel
    {
        Task SendAsync(INotification notification);

        Task SendAsync(INotification notification, TimeSpan visibilityTimeout);

        Task<INotification[]> ReceiveNotificationsAsync();
    }
}
