using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications.Listeners
{
    public interface INotificationListenerSubscription
    {
        void Start(IEventStoreConnection connection);

        void Stop();

        Task<IEventStreamConsumer> CreateSubscriptionConsumerAsync(string streamName);

        Task RetryNotificationProcessingAsync(INotification notification);
    }
}
