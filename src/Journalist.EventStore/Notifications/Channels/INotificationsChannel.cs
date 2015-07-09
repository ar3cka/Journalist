using System.IO;
using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Channels
{
    public interface INotificationsChannel
    {
        Task SendAsync(Stream bytes);

        Task<Stream[]> ReceiveNotificationsAsync();
    }
}
