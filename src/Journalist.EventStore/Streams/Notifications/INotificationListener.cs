using System.Threading.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public interface INotificationListener
    {
        Task OnSubscriptionStarted();

        Task OnSubscriptionStopped();

        Task OnEventStreamUpdatedAsync(EventStreamUpdated notification);
    }
}
