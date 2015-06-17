using System;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Queues
{
    public static class CloudQueueExtensions
    {
        public static Task UpdateMessageAsync(this ICloudQueue queue, ICloudQueueMessage message, byte[] content, TimeSpan visibilityTimeout)
        {
            Require.NotNull(queue, "queue");
            Require.NotNull(message, "message");

            return queue.UpdateMessageAsync(message.MessageId, message.PopReceipt, content, visibilityTimeout);
        }

        public static Task UpdateMessageAsync(this ICloudQueue queue, ICloudQueueMessage message, byte[] content)
        {
            Require.NotNull(queue, "queue");
            Require.NotNull(message, "message");

            return queue.UpdateMessageAsync(message.MessageId, message.PopReceipt, content);
        }

        public static Task UpdateMessageAsync(this ICloudQueue queue, ICloudQueueMessage message, TimeSpan visibilityTimeout)
        {
            Require.NotNull(queue, "queue");
            Require.NotNull(message, "message");

            return queue.UpdateMessageAsync(message.MessageId, message.PopReceipt, visibilityTimeout);
        }

        public static Task DeleteMessageAsync(this ICloudQueue queue, ICloudQueueMessage message)
        {
            Require.NotNull(queue, "queue");
            Require.NotNull(message, "message");

            return queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }
    }
}
