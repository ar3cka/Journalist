using Journalist.EventStore.Connection;

namespace Journalist.EventStore.Notifications.Listeners
{
    public interface INotificationListenerSubscription
    {
        void Start(IEventStoreConnection connection);

        void Stop();

        IEventStoreConnection Connection { get; }
    }
}
