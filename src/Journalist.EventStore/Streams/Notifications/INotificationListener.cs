using System.Threading.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public interface INotificationListener
    {
        Task OnEventStreamUpdatedAsync(EventStreamUpdated notification);
    }
}