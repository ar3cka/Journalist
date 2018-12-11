using Journalist.EventStore.Notifications.Listeners;

namespace Journalist.EventStore.Configuration
{
    public interface INotificationProcessingConfiguration
    {
        IEventStoreConnectionConfiguration EnableProcessing();

        IEventStoreConnectionConfiguration Subscribe(INotificationListener listener);
    }
}
