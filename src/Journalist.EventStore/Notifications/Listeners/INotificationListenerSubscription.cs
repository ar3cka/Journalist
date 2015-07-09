namespace Journalist.EventStore.Notifications.Listeners
{
    public interface INotificationListenerSubscription
    {
        void Start();

        void Stop();
    }
}
