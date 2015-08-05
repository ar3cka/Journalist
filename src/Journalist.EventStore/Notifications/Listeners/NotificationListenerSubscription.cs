using System;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Serilog;

namespace Journalist.EventStore.Notifications.Listeners
{
    public class NotificationListenerSubscription : INotificationListenerSubscription
    {
        private static readonly ILogger s_logger = Log.ForContext<NotificationListenerSubscription>();

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
                        await ProcessNotificationAsync(notification, m_listener);
                    }
                }
                else
                {
                    await ProcessNotificationAsync(notification, m_listener);
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

            return m_connection.CreateStreamConsumerAsync(config => config
                .UseConsumerId(m_subscriptionConsumerId)
                .ReadFromStream(streamName)
                .AutoCommitProcessedStreamPosition(false));
        }

        public Task DefferNotificationAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            return m_hub.NotifyAsync(notification.SendTo(m_subscriptionConsumerId));
        }

        private static async Task ProcessNotificationAsync(dynamic notification, INotificationListener listener)
        {
            try
            {
                await listener.On(notification);
            }
            catch (Exception exception)
            {
                s_logger.Error(exception, "Processing notification {@Notification} failed.", notification);
            }
        }
    }
}
