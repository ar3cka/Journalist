using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Internals;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Journalist.WindowsAzure.Storage.Queues
{
    public class CloudQueueAdapter : CloudEntityAdapter<CloudQueue>, ICloudQueue
    {
        public CloudQueueAdapter(Func<CloudQueue> entityFactory) : base(entityFactory)
        {
        }

        public Task AddMessageAsync(byte[] content)
        {
            return CloudEntity.AddMessageAsync(new CloudQueueMessage(content));
        }

        public Task AddMessageAsync(byte[] content, TimeSpan visibilityTimeout)
        {
            return CloudEntity.AddMessageAsync(
                message: new CloudQueueMessage(content),
                timeToLive: null,
                initialVisibilityDelay: visibilityTimeout,
                options: null,
                operationContext: null);
        }

        public async Task<ICloudQueueMessage> GetMessageAsync()
        {
            var message = await CloudEntity.GetMessageAsync();

            if (message == null)
            {
                return null;
            }

            return new CloudQueueMessageAdapter(
                message.Id,
                message.PopReceipt,
                message.DequeueCount,
                message.AsBytes);
        }

        public async Task<ICloudQueueMessage> GetMessageAsync(TimeSpan visibilityTimeout)
        {
            var message = await CloudEntity.GetMessageAsync(
                visibilityTimeout,
                null,
                null);

            if (message == null)
            {
                return null;
            }

            return new CloudQueueMessageAdapter(
                message.Id,
                message.PopReceipt,
                message.DequeueCount,
                message.AsBytes);
        }

        public Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync()
        {
            return GetMessagesAsync(CloudQueueMessage.MaxNumberOfMessagesToPeek);
        }

        public async Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync(int messageCount)
        {
            var messages = await CloudEntity.GetMessagesAsync(messageCount);

            var result = new List<ICloudQueueMessage>(messageCount);
            result.AddRange(
                messages.Select(message =>
                    new CloudQueueMessageAdapter(
                        message.Id,
                        message.PopReceipt,
                        message.DequeueCount,
                        message.AsBytes)));

            return result;
        }

        public Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync(TimeSpan visibilityTimeout)
        {
            return GetMessagesAsync(CloudQueueMessage.MaxNumberOfMessagesToPeek, visibilityTimeout);
        }

        public async Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync(int messageCount, TimeSpan visibilityTimeout)
        {
            var messages = await CloudEntity.GetMessagesAsync(
                messageCount,
                visibilityTimeout,
                null,
                null);

            var result = new List<ICloudQueueMessage>(CloudQueueMessage.MaxNumberOfMessagesToPeek);
            result.AddRange(
                messages.Select(message =>
                    new CloudQueueMessageAdapter(
                        message.Id,
                        message.PopReceipt,
                        message.DequeueCount,
                        message.AsBytes)));

            return result;
        }

        public Task UpdateMessageAsync(string messageId, string popReceipt, byte[] content, TimeSpan visibilityTimeout)
        {
            Require.NotEmpty(messageId, "messageId");
            Require.NotEmpty(popReceipt, "popReceipt");
            Require.NotNull(content, "content");

            var message = new CloudQueueMessage(messageId, popReceipt);
            message.SetMessageContent(content);

            return CloudEntity.UpdateMessageAsync(
                message,
                visibilityTimeout,
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);
        }

        public Task UpdateMessageAsync(string messageId, string popReceipt, byte[] content)
        {
            Require.NotEmpty(messageId, "messageId");
            Require.NotEmpty(popReceipt, "popReceipt");
            Require.NotNull(content, "content");

            var message = new CloudQueueMessage(messageId, popReceipt);
            message.SetMessageContent(content);

            return CloudEntity.UpdateMessageAsync(
                message,
                TimeSpan.Zero,
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);
        }

        public Task UpdateMessageAsync(string messageId, string popReceipt, TimeSpan visibilityTimeout)
        {
            Require.NotEmpty(messageId, "messageId");
            Require.NotEmpty(popReceipt, "popReceipt");

            var message = new CloudQueueMessage(messageId, popReceipt);

            return CloudEntity.UpdateMessageAsync(
                message,
                visibilityTimeout,
                MessageUpdateFields.Visibility);
        }

        public Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            Require.NotEmpty(messageId, "messageId");
            Require.NotEmpty(popReceipt, "popReceipt");

            return CloudEntity.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}
