using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Streams;
using Journalist.Extensions;
using Serilog;
using Serilog.Events;

namespace Journalist.EventStore.Notifications.Listeners
{
    public class NotificationListenerSubscription : INotificationListenerSubscription
    {
        private const int MAX_ATTEMPT_COUNT= 10;

        private static readonly ILogger s_logger = Log.ForContext<NotificationListenerSubscription>();

        private readonly EventStreamConsumerId m_subscriptionConsumerId;
        private readonly INotificationsChannel m_notificationsChannel;
        private readonly INotificationListener m_listener;
        private readonly CountdownEvent m_countdown;
        private IEventStoreConnection m_connection;

        public NotificationListenerSubscription(
            EventStreamConsumerId subscriptionConsumerId,
            INotificationsChannel notificationsChannel,
            INotificationListener listener)
        {
            Require.NotNull(subscriptionConsumerId, "subscriptionConsumerId");
            Require.NotNull(notificationsChannel, "notificationsChannel");
            Require.NotNull(listener, "listener");

            m_subscriptionConsumerId = subscriptionConsumerId;
            m_notificationsChannel = notificationsChannel;
            m_listener = listener;
            m_countdown = new CountdownEvent(0);
        }

        public async Task HandleNotificationAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            if (notification.IsAddressed)
            {
                if (notification.IsAddressedTo(m_subscriptionConsumerId))
                {
                    await ProcessNotificationAsync(notification);
                }
            }
            else
            {
                await ProcessNotificationAsync(notification);
            }
        }

        public void Start(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            m_connection = connection;
            m_listener.OnSubscriptionStarted(this);
            m_countdown.Reset(1);
        }

        public void Stop()
        {
            Ensure.False(m_countdown.IsSet, "Subscription has not been not started.");

            m_countdown.Signal();
            m_listener.OnSubscriptionStopped();
            m_countdown.Wait();

            m_connection = null;
        }

        public Task<IEventStreamConsumer> CreateSubscriptionConsumerAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            Ensure.False(m_countdown.IsSet, "Subscription is not activated.");

            return m_connection.CreateStreamConsumerAsync(config => config
                .UseConsumerId(m_subscriptionConsumerId)
                .ReadFromStream(streamName)
                .AutoCommitProcessedStreamPosition(false));
        }

        public async Task RetryNotificationProcessingAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            if (notification.DeliveryCount < MAX_ATTEMPT_COUNT)
            {
                var retryNotification = notification.SendTo(m_subscriptionConsumerId);

                if (s_logger.IsEnabled(LogEventLevel.Debug))
                {
                    s_logger.Debug(
                        "Sending retry notification ({RetryNotificationId}, {NotificationType} to consumer {SubscriptionConsumerId}. " +
                        "Source notification: {SourceNotificationId}.",
                        retryNotification.NotificationId,
                        retryNotification.NotificationType,
                        m_subscriptionConsumerId,
                        notification.NotificationId);
                }

                await m_notificationsChannel.SendAsync(retryNotification);
            }
            else
            {
                s_logger.Error(
                    "Number of processing attempt has been exceeded. " +
                        "Listener type: {ListenerType}. " +
                        "NotificationId: {Notification}. " +
                        "NotificationType: {NotificationType}. " +
                        "Delivery count: {DeliveryCount}. " +
                        "Maximum deliver count: {MaxDeliveryCount}",
                    m_listener.GetType(),
                    notification.NotificationId,
                    notification.NotificationType,
                    notification.DeliveryCount.ToInvariantString(),
                    MAX_ATTEMPT_COUNT.ToInvariantString());
            }
        }

        private async Task ProcessNotificationAsync(dynamic notification)
        {
            try
            {
                if (m_countdown.TryAddCount())
                {
                    await m_listener.On(notification);
                }
            }
            catch (Exception exception)
            {
                s_logger.Error(exception, "Processing notification {@Notification} failed.", notification);
            }
            finally
            {
                if (!m_countdown.IsSet)
                {
                    m_countdown.Signal();
                }
            }
        }
    }
}
