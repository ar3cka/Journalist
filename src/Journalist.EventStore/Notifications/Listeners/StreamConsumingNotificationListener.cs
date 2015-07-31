using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
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

            m_subscription = subscription;
        }

        public void OnSubscriptionStopped()
        {
        }

        public async Task On(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            var consumer = await m_subscription.CreateSubscriptionConsumerAsync(notification.StreamName);
            var retryProcessing = true;
            if (await consumer.ReceiveEventsAsync() && await TryProcessEventFromConsumerAsync(consumer))
            {
                await consumer.CommitProcessedStreamVersionAsync();
                retryProcessing = false;
            }

            if (retryProcessing)
            {
                await m_subscription.DefferNotificationAsync(notification);
            }

            await consumer.CloseAsync();
        }

        protected abstract Task<bool> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer);

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
