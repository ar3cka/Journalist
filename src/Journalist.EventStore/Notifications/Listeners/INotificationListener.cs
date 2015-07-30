using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications.Listeners
{
    public interface INotificationListener
    {
        void OnSubscriptionStarted(INotificationListenerSubscription subscription);

        void OnSubscriptionStopped();

        Task On(EventStreamUpdated notification);
    }
}
