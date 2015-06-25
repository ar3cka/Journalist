namespace Journalist.WindowsAzure.Storage.Queues
{
    public interface ICloudQueueMessage
    {
        byte[] Content { get; }

        int DequeueCount { get; }

        string PopReceipt { get; }

        string MessageId { get; }
    }
}
