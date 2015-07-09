using System.Threading.Tasks;
using Journalist.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public class NotificationListenerSubscription : INotificationListenerSubscription
    {
        private readonly INotificationListener m_listener;
        private bool m_active;

        public NotificationListenerSubscription(INotificationListener listener)
        {
            m_listener = listener;
        }

        public Task HandleNotificationAsync(dynamic notification)
        {
            if (m_active)
            {
                return m_listener.OnAsync(notification);
            }

            return TaskDone.Done;
        }

        public void Start()
        {
            m_listener.OnSubscriptionStarted();

            m_active = true;
        }

        public void Stop()
        {
            m_active = false;

            m_listener.OnSubscriptionStopped();
        }
    }
}
