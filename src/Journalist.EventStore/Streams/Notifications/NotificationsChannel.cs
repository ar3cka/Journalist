using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Journalist.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public class NotificationsChannel : INotificationsChannel
    {
        public Task SendAsync(Stream bytes)
        {
            return TaskDone.Done;
        }

        public Task<List<Stream>> ReceiveNotificationsAsync()
        {
            return Task.FromResult(new List<Stream>(0));
        }
    }
}