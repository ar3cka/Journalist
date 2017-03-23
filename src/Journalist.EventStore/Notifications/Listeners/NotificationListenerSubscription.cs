using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Processing;
using Journalist.EventStore.Streams;
using Journalist.Extensions;
using Serilog;
using Serilog.Events;

namespace Journalist.EventStore.Notifications.Listeners
{
    public class NotificationListenerSubscription : INotificationListenerSubscription, INotificationHandler
    {
        private static readonly ILogger s_logger = Log.ForContext<NotificationListenerSubscription>();

        private readonly INotificationsChannel m_notificationsChannel;
        private readonly INotificationListener m_listener;
	    private readonly IFailedNotificationsHub m_failedNotificationsHub;
	    private readonly CountdownEvent m_processingCountdown;
        private IEventStoreConnection m_connection;

        public NotificationListenerSubscription(
            INotificationsChannel notificationsChannel,
            INotificationListener listener,
			IFailedNotificationsHub failedNotificationsHub)
        {
            Require.NotNull(notificationsChannel, nameof(notificationsChannel));
            Require.NotNull(listener, nameof(listener));
			Require.NotNull(failedNotificationsHub, nameof(failedNotificationsHub));

            m_notificationsChannel = notificationsChannel;
            m_listener = listener;
	        m_failedNotificationsHub = failedNotificationsHub;
	        m_processingCountdown = new CountdownEvent(0);
        }

        public async Task HandleNotificationAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            if (notification.IsAddressed)
            {
                if (notification.IsAddressedTo(m_listener))
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

        public Task<IEventStreamConsumer> CreateSubscriptionConsumerAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            Ensure.False(m_processingCountdown.IsSet, "Subscription is not activated.");

            return m_connection.CreateStreamConsumerAsync(config => config
                .WithName(GetConsumerId())
                .ReadStream(streamName)
                .AutoCommitProcessedStreamPosition(false));
        }

        public async Task RetryNotificationProcessingAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            if (notification.DeliveryCount < Constants.Settings.MAX_NOTIFICATION_PROCESSING_ATTEMPT_COUNT)
            {
                var retryNotification = notification.SendTo(m_listener);
                var deliverTimeout = TimeSpan.FromSeconds(
                    notification.DeliveryCount * Constants.Settings.NOTIFICATION_RETRY_DELIVERY_TIMEOUT_MULTIPLYER_SEC);

                if (s_logger.IsEnabled(LogEventLevel.Debug))
                {
                    s_logger.Debug(
                        "Sending retry notification ({RetryNotificationId}, {NotificationType}) with timeout {Timeout} " +
                        "to consumer {ListenerType}. " +
                        "Source notification: {SourceNotificationId}.",
                        retryNotification.NotificationId,
                        retryNotification.NotificationType,
                        deliverTimeout.ToInvariantString(),
                        GetConsumerId(),
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
                    Constants.Settings.MAX_NOTIFICATION_PROCESSING_ATTEMPT_COUNT.ToInvariantString());

	            await m_failedNotificationsHub.PutToFailedAsync(notification);
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
                    var redeliveredNotification = notification.RedeliverTo(m_listener);

                    s_logger.Warning(
                        "Trying to redeliver notification (RedeliverNotificationId}, {NotificationType}) " +
                        "to consumer {ListenerType} because subscription is stopping. " +
                        "Source notification: {SourceNotificationId}.",
                        redeliveredNotification.NotificationId,
                        redeliveredNotification.NotificationType,
                        GetConsumerId(),
                        notification.NotificationId);

                    await m_notificationsChannel.SendAsync(redeliveredNotification);
                }
            }
            finally
            {
                if (!m_processingCountdown.IsSet)
                {
                    m_processingCountdown.Signal();
                }
            }
        }

        private string GetConsumerId()
        {
            return m_listener.GetType().FullName;
        }
    }
}
