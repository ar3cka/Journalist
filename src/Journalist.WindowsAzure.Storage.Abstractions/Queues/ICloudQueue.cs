using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Queues
{
    public interface ICloudQueue
    {
        Task AddMessageAsync(byte[] content);

        Task<ICloudQueueMessage> GetMessageAsync();

        Task<IReadOnlyList<ICloudQueueMessage>> GetMessagesAsync();

        Task DeleteMessageAsync(string messageId, string popReceipt);
    }
}
