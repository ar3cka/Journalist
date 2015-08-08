using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Serilog;

namespace Journalist.EventStore.Notifications.Listeners
{
    public abstract class StreamConsumingNotificationListener : INotificationListener
    {
        private readonly static object s_lock = new object();
        private readonly CountdownEvent m_processingCountdown = new CountdownEvent(0);
        private ILogger m_logger;

        private INotificationListenerSubscription m_subscription;

        public void OnSubscriptionStarted(INotificationListenerSubscription subscription)
        {
            Require.NotNull(subscription, "subscription");

            AssertSubscriptionWasNotBound();

            m_subscription = subscription;
            m_processingCountdown.Reset(1);
        }

        public void OnSubscriptionStopped()
        {
            m_processingCountdown.Signal();
            m_processingCountdown.Wait();

            m_subscription = null;
        }

        public async Task On(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            AssertSubscriptionWasBound();

            m_processingCountdown.AddCount();

            try
            {
                var consumer = await m_subscription.CreateSubscriptionConsumerAsync(notification.StreamName);
                await ReceiveAndProcessEventsAsync(notification, consumer);
                await consumer.CloseAsync();
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
            finally
            {
                m_processingCountdown.Signal();
            }
        }

        protected abstract Task<bool> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer);

        private async Task ReceiveAndProcessEventsAsync(EventStreamUpdated notification, IEventStreamConsumer consumer)
        {
            var retryProcessing = true;
            var receivingResult = await consumer.ReceiveEventsAsync();

            if (receivingResult == ReceivingResultCode.EventsReceived && await TryProcessEventFromConsumerAsync(consumer))
            {
                await consumer.CommitProcessedStreamVersionAsync();
                retryProcessing = false;
            }
            else if (receivingResult == ReceivingResultCode.EmptyStream)
            {
                retryProcessing = false;
            }

            if (retryProcessing)
            {
                ListenerLogger.Warning(
                    "Processing notification ({NotificationId}, {NotificationType}) was unsuccessful (Code). Going to try later.",
                    notification.NotificationId,
                    notification.NotificationType,
                    receivingResult);

                await m_subscription.RetryNotificationProcessingAsync(notification);
            }
        }

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
