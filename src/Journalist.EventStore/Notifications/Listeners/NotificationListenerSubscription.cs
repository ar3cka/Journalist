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
        private static readonly ILogger s_logger = Log.ForContext<NotificationListenerSubscription>();

        private readonly EventStreamConsumerId m_subscriptionConsumerId;
        private readonly INotificationsChannel m_notificationsChannel;
        private readonly INotificationListener m_listener;
        private readonly CountdownEvent m_processingCountdown;
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
            m_processingCountdown = new CountdownEvent(0);
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
            m_processingCountdown.Reset(1);
        }

        public void Stop()
        {
            Ensure.False(m_processingCountdown.IsSet, "Subscription has not been not started.");

            m_processingCountdown.Signal();
            m_processingCountdown.Wait();
            m_listener.OnSubscriptionStopped();

            m_connection = null;
        }

        public Task<IEventStreamConsumer> CreateSubscriptionConsumerAsync(string streamName, bool readFromEnd)
        {
            Require.NotEmpty(streamName, "streamName");

            Ensure.False(m_processingCountdown.IsSet, "Subscription is not activated.");

            return m_connection.CreateStreamConsumerAsync(config => config
                .UseConsumerId(m_subscriptionConsumerId)
                .ReadStream(streamName, readFromEnd)
                .AutoCommitProcessedStreamPosition(false));
        }

        public async Task RetryNotificationProcessingAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            if (notification.DeliveryCount < Constants.Settings.DEFAULT_MAX_NOTIFICATION_PROCESSING_ATTEMPT_COUNT)
            {
                var retryNotification = notification.SendTo(m_subscriptionConsumerId);
                var deliverTimeout = TimeSpan.FromSeconds(
                    notification.DeliveryCount * Constants.Settings.DEFAULT_NOTIFICATION_RETRY_DELIVERY_TIMEOUT_MULTIPLYER_SEC);

                if (s_logger.IsEnabled(LogEventLevel.Debug))
                {
                    s_logger.Debug(
                        "Sending retry notification ({RetryNotificationId}, {NotificationType}) with timeout {Timeout} " +
                        "to consumer {SubscriptionConsumerId}. " +
                        "Source notification: {SourceNotificationId}.",
                        retryNotification.NotificationId,
                        retryNotification.NotificationType,
                        deliverTimeout.ToInvariantString(),
                        m_subscriptionConsumerId,
                        notification.NotificationId);
                }

                await m_notificationsChannel.SendAsync(retryNotification, deliverTimeout);
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
                    Constants.Settings.DEFAULT_MAX_NOTIFICATION_PROCESSING_ATTEMPT_COUNT.ToInvariantString());
            }
        }

        private async Task ProcessNotificationAsync(INotification notification)
        {
            try
            {
                if (m_processingCountdown.TryAddCount())
                {
                    await m_listener.On((dynamic)notification);
                }
                else
                {
                    var redeliveredNotification = notification.RedeliverTo(m_subscriptionConsumerId);

                    s_logger.Warning(
                        "Trying to redeliver notification (RedeliverNotificationId}, {NotificationType}) " +
                        "to consumer {SubscriptionConsumerId} because subscription is stopping. " +
                        "Source notification: {SourceNotificationId}.",
                        redeliveredNotification.NotificationId,
                        redeliveredNotification.NotificationType,
                        m_subscriptionConsumerId,
                        notification.NotificationId);

                    await m_notificationsChannel.SendAsync(redeliveredNotification);
                }
            }
            catch (Exception exception)
            {
                s_logger.Error(exception, "Processing notification {@Notification} failed.", notification);
            }
            finally
            {
                if (!m_processingCountdown.IsSet)
                {
                    m_processingCountdown.Signal();
                }
            }
        }
    }
}
