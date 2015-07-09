using System.IO;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.Tasks;

namespace Journalist.EventStore.Notifications.Channels
{
    public class NotificationsChannel : INotificationsChannel
    {
        public Task SendAsync(Stream bytes)
        {
            return TaskDone.Done;
        }

        public Task<Stream[]> ReceiveNotificationsAsync()
        {
            return Task.FromResult(EmptyArray.Get<Stream>());
        }
    }
}
