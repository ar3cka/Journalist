using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications.Listeners
{
    public class EventConsumingNotificationListener : INotificationListener
    {
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
            if (await consumer.ReceiveEventsAsync())
            {
            }
            else
            {
                await m_subscription.DefferNotificationAsync(notification);
            }

            await consumer.CloseAsync();
        }
    }
}
