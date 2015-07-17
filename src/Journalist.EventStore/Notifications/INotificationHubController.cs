namespace Journalist.EventStore.Notifications
{
    public interface INotificationHubController
    {
        void StartHub(INotificationHub notificationHub);

        void StopHub(INotificationHub notificationHub);
    }
}
