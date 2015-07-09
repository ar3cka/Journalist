using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public interface INotificationsChannel
    {
        Task SendAsync(Stream bytes);

        Task<List<Stream>> ReceiveNotificationsAsync();
    }
}