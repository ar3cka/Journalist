using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Queues
{
    public interface ICloudQueue
    {
        Task AddMessageAsync(byte[] content);

        Task AddMessageAsync(byte[] content, TimeSpan visibilityTimeout);

        Task<ICloudQueueMessage> GetMessageAsync();

        Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync();

        Task UpdateMessageAsync(string messageId, string popReceipt, byte[] content, TimeSpan visibilityTimeout);

        Task UpdateMessageAsync(string messageId, string popReceipt, byte[] content);

        Task UpdateMessageAsync(string messageId, string popReceipt, TimeSpan visibilityTimeout);

        Task DeleteMessageAsync(string messageId, string popReceipt);
    }
}
