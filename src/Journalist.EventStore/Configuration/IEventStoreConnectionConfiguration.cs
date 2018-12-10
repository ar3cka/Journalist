namespace Journalist.EventStore.Configuration
{
    public interface IEventStoreConnectionConfiguration
    {
        IEventStoreConnectionConfiguration UseStorage(
            string storageConnectionString,
            string journalTableName = Constants.StorageEntities.EVENT_JOURNAL_TABLE_NAME,
            string eventStoreDeploymentTableName = Constants.StorageEntities.EVENT_STORE_DEPLOYMENT_TABLE_NAME,
            string notificationQueueName = Constants.StorageEntities.NOTIFICATION_QUEUE_NAME,
            int notificationQueuePartitionCount = Constants.Settings.NOTIFICATION_QUEUE_PARTITION_COUNT,
            string failedNotificationsTableName = Constants.StorageEntities.FAILED_NOTIFICATIONS_TABLE_NAME,
            string streamConsumerSessionsBlobContainerName = Constants.StorageEntities.EVENT_CONSUMER_SESSIONS_BLOB_CONTAINER_NAME,
            string pendingNotificationsTableName = Constants.StorageEntities.PENDING_NOTIFICATIONS_TABLE_NAME,
            string pendingNotificationsChaserExclusiveAccessLockBlobContainerName = Constants.StorageEntities.PENDING_NOTIFICATIONS_CHASER_EXCLUSIVE_ACCESS_LOCK_BLOB_CONTAINER_NAME,
            string pendingNotificationsChaserExclusiveAccessLockBlobName = Constants.StorageEntities.PENDING_NOTIFICATIONS_CHASER_EXCLUSIVE_ACCESS_LOCK_BLOB_NAME);

        IEventMutationPipelineConfiguration Mutate { get; }

        INotificationProcessingConfiguration Notifications { get; }
    }
}
