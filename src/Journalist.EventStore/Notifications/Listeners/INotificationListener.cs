using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications.Listeners
{
    public interface INotificationListener
    {
        Task OnSubscriptionStarted();

        Task OnSubscriptionStopped();

        Task OnAsync(EventStreamUpdated notification);
    }
}
