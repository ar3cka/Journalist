using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public class NotificationHub : INotificationHub
    {
        private readonly List<NotificationListenerSubscription> m_subscriptions = new List<NotificationListenerSubscription>();

        public async Task NotifyAsync(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            foreach (var subscription in m_subscriptions)
            {
                await subscription.HandleNotificationAsync(notification);
            }
        }

        public INotificationListenerSubscription Subscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            var result = new NotificationListenerSubscription(listener);
            m_subscriptions.Add(result);

            return result;
        }
    }
}
