namespace Journalist.EventStore.Streams.Notifications
{
    public interface INotificationListenerSubscription
    {
        void Start();

        void Stop();
    }
}
