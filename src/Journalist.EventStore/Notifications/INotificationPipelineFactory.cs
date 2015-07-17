namespace Journalist.EventStore.Notifications
{
    public interface INotificationPipelineFactory
    {
        INotificationHub CreateHub();

        INotificationHubController CreateHubController();
    }
}
