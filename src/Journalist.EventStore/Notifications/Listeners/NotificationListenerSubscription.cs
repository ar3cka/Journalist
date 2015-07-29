using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.Tasks;

namespace Journalist.EventStore.Notifications.Listeners
{
    public class NotificationListenerSubscription : INotificationListenerSubscription
    {
        private readonly INotificationListener m_listener;
        private bool m_active;
        private IEventStoreConnection m_connection;

        public NotificationListenerSubscription(INotificationListener listener)
        {
            m_listener = listener;
        }

        public Task HandleNotificationAsync(dynamic notification)
        {
            if (m_active)
            {
                return m_listener.On(notification);
            }

            return TaskDone.Done;
        }

        public void Start(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            m_connection = connection;

            m_listener.OnSubscriptionStarted(this);

            m_active = true;
        }

        public void Stop()
        {
            m_connection = null;
            m_active = false;

            m_listener.OnSubscriptionStopped();
        }

        public IEventStoreConnection Connection
        {
            get
            {
                Ensure.True(m_connection != null, "Subscription is not activated.");

                return m_connection;
            }
        }
    }
}
