namespace Journalist.EventStore.Configuration
{
    public interface IEventStoreConnectionConfiguration
    {
        IEventStoreConnectionConfiguration UseStorage(
            string storageConnectionString,
            string journalTableName = Constants.StorageEntities.EVENT_JOURNAL_TABLE_NAME,
            string notificationQueueName = Constants.StorageEntities.NOTIFICATION_QUEUE_NAME,
            string streamConsumerSessionsBlobName = Constants.StorageEntities.EVENT_STREAM_CONSUMER_SESSIONS_BLOB_NAME);

        IEventMutationPipelineConfiguration Mutate { get; }
    }
}
