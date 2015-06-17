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

        public Task AddMessageAsync(byte[] bytes)
        {
            return CloudEntity.AddMessageAsync(new CloudQueueMessage(bytes));
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

        public async Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync()
        {
            var messages = await CloudEntity.GetMessagesAsync(CloudQueueMessage.MaxNumberOfMessagesToPeek);

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
    }
}
