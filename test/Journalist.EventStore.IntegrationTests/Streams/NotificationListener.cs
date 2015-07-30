using System.Collections.Concurrent;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.Tasks;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public abstract class NotificationListener : INotificationListener
    {
        public readonly BlockingCollection<EventStreamUpdated> Notifications =
            new BlockingCollection<EventStreamUpdated>(new ConcurrentQueue<EventStreamUpdated>());

        public void OnSubscriptionStarted(INotificationListenerSubscription subscription)
        {
            Started = true;
        }

        public void OnSubscriptionStopped()
        {
            Started = false;
        }

        public Task On(EventStreamUpdated notification)
        {
            Notifications.Add(notification);

            return TaskDone.Done;
        }

        public bool Started { get; set; }
    }
}
