using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Journalist.Tasks;
using Serilog;

namespace Journalist.EventStore.Notifications.Listeners
{
    public abstract class StreamConsumingNotificationListener : INotificationListener
    {
        private static readonly object s_lock = new object();
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

            var retryProcessing = false;
            try
            {
                if (!await NeedToProcessAsync(notification))
                {
                    return;
                }

                var consumer = await m_subscription.CreateSubscriptionConsumerAsync(notification.StreamName);

                retryProcessing = await ReceiveAndProcessEventsAsync(notification, consumer);
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

                retryProcessing = true;
            }
            finally
            {
                m_processingCountdown.Signal();
            }

            if (retryProcessing)
            {
                await m_subscription.RetryNotificationProcessingAsync(notification);
            }
        }

        protected virtual Task<bool> NeedToProcessAsync(EventStreamUpdated notification)
        {
            return true.YieldTask();
        }

        protected abstract Task<EventProcessingResult> TryProcessEventFromConsumerAsync(
            IEventStreamConsumer consumer,
            StreamVersion notificationStreamVersion);

        private async Task<bool> ReceiveAndProcessEventsAsync(EventStreamUpdated notification, IEventStreamConsumer consumer)
        {
            var retryProcessing = true;
            var commitProcessing = false;
            var receivingResult = await consumer.ReceiveEventsAsync();

            if (receivingResult == ReceivingResultCode.EventsReceived)
            {
                var processingResult = await TryProcessEventFromConsumerAsync(consumer, notification.ToVersion);
                retryProcessing = !processingResult.IsSuccessful;
                commitProcessing = processingResult.ShouldCommitProcessing;
            }
            else if (receivingResult == ReceivingResultCode.EmptyStream)
            {
                retryProcessing = false;
            }

            if (commitProcessing)
            {
                await consumer.CommitProcessedStreamVersionAsync();
            }
            if (retryProcessing)
            {
                ListenerLogger.Warning(
                    "Processing notification ({NotificationId}, {NotificationType}) was unsuccessful {Code}. Going to try later.",
                    notification.NotificationId,
                    notification.NotificationType,
                    receivingResult);
            }

            return retryProcessing;
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
