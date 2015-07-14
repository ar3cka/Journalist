using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Queues;

namespace Journalist.EventStore.Notifications.Channels
{
    public class NotificationsChannel : INotificationsChannel
    {
        private readonly ICloudQueue m_queue;

        public NotificationsChannel(ICloudQueue queue)
        {
            Require.NotNull(queue, "queue");

            m_queue = queue;
        }

        public async Task SendAsync(Stream bytes)
        {
            Require.NotNull(bytes, "bytes");

            using (var stream = new MemoryStream(new byte[bytes.Length]))
            {
                bytes.CopyTo(stream);

                await m_queue.AddMessageAsync(stream.ToArray());
            }
        }

        public async Task<Stream[]> ReceiveNotificationsAsync()
        {
            var messages = await m_queue.GetMessagesAsync();

            var result = new List<Stream>();
            foreach (var message in messages)
            {
                result.Add(new MemoryStream(message.Content));
                await m_queue.DeleteMessageAsync(message);
            }

            return result.ToArray();
        }
    }
}
