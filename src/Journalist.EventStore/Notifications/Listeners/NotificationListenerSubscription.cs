using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications.Listeners
{
    public class NotificationListenerSubscription : INotificationListenerSubscription
    {
        private readonly EventStreamConsumerId m_subscriptionConsumerId;
        private readonly INotificationListener m_listener;
        private readonly INotificationHub m_hub;
        private bool m_active;
        private IEventStoreConnection m_connection;

        public NotificationListenerSubscription(
            EventStreamConsumerId subscriptionConsumerId,
            INotificationHub hub,
            INotificationListener listener)
        {
            Require.NotNull(subscriptionConsumerId, "subscriptionConsumerId");
            Require.NotNull(hub, "hub");
            Require.NotNull(listener, "listener");

            m_subscriptionConsumerId = subscriptionConsumerId;
            m_hub = hub;
            m_listener = listener;
        }

        public async Task HandleNotificationAsync(dynamic notification)
        {
            if (m_active)
            {
                var notificationInterface = (INotification)notification;
                if (notificationInterface.IsAddressed)
                {
                    if (notificationInterface.IsAddressedTo(m_subscriptionConsumerId))
                    {
                        await m_listener.On(notification);
                    }
                }
                else
                {
                        await m_listener.On(notification);
                }
            }
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

        public Task<IEventStreamConsumer> CreateSubscriptionConsumerAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            Ensure.True(m_active, "Subscription is not activated.");

            return m_connection.CreateStreamConsumerAsync(streamName, m_subscriptionConsumerId);
        }

        public Task DefferNotificationAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            return m_hub.NotifyAsync(notification.SendTo(m_subscriptionConsumerId));
        }
    }
}
