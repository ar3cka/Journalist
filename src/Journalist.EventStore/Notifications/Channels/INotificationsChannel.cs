using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications.Channels
{
    public interface INotificationsChannel
    {
        Task SendAsync(INotification notification);

        Task<INotification[]> ReceiveNotificationsAsync();
    }
}
