using System;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Journalist.Extensions;
using Serilog;

namespace Journalist.EventStore.Notifications.Listeners
{
    public abstract class StreamConsumingNotificationListener : INotificationListener
    {
        private readonly static object s_lock = new object();
        private ILogger m_logger;

        private INotificationListenerSubscription m_subscription;

        public void OnSubscriptionStarted(INotificationListenerSubscription subscription)
        {
            Require.NotNull(subscription, "subscription");

            AssertSubscriptionWasNotBound();

            m_subscription = subscription;
        }

        public void OnSubscriptionStopped()
        {
            m_subscription = null;
        }

        public async Task On(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            AssertSubscriptionWasBound();

            var consumer = await m_subscription.CreateSubscriptionConsumerAsync(notification.StreamName);

            try
            {
                var retryProcessing = true;
                if (await consumer.ReceiveEventsAsync() && await TryProcessEventFromConsumerAsync(consumer))
                {
                    await consumer.CommitProcessedStreamVersionAsync();
                    retryProcessing = false;
                }

                if (retryProcessing)
                {
                    ListenerLogger.Warning(
                        "Processing notification ({NotificationId}, {NotificationType}) was unsuccessful. Going to try later.",
                        notification.NotificationId,
                        notification.NotificationType);

                    await m_subscription.RetryNotificationProcessingAsync(notification);
                }
            }
            catch (Exception exception)
            {
                ListenerLogger.Error(
                    exception,
                    "Processing notification ({NotificationId}, {NotificationType}) from {Stream} failed.",
                    notification.NotificationId,
                    notification.NotificationType,
                    notification.StreamName);
            }

            await consumer.CloseAsync();
        }

        protected abstract Task<bool> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer);

        private void AssertSubscriptionWasNotBound()
        {
            Ensure.True(m_subscription == null, "Subscription was bound to listener.");
        }

        private void AssertSubscriptionWasBound()
        {
            Ensure.True(m_subscription != null, "Subscription was not bound to listener.");
        }

        protected ILogger ListenerLogger
        {
            get
            {
                if (m_logger == null)
                {
                    lock (s_lock)
                    {
                        if (m_logger == null)
                        {
                            var logger = Log.ForContext(GetType());
                            m_logger = logger;
                        }
                    }
                }

                return m_logger;
            }
        }
    }
}
