namespace Journalist.EventStore
{
    internal class Constants
    {
        public class Settings
        {
            public const int SESSION_LOCK_TIMEOUT_MINUTES = 15;
            public const int EVENT_SLICE_SIZE = 100;

            public const int MAX_NOTIFICATION_PROCESSING_COUNT = 100;
            public const int MAX_NOTIFICATION_PROCESSING_ATTEMPT_COUNT= 10;
            public const int NOTIFICATION_MESSAGE_LOCK_TIMEOUT_MINUTES = 15;
            public const int NOTIFICATION_RETRY_DELIVERY_TIMEOUT_MULTIPLYER_SEC = 2;
            public const int NOTIFICATION_QUEUE_PARTITION_COUNT = 32;

            public const int PENDING_NOTIFICATIONS_CHASER_INITIAL_TIMEOUT_IN_MINUTES = 1;
            public const int PENDING_NOTIFICATIONS_CHASER_TIMEOUT_MULTIPLIER = 2;
            public const int PENDING_NOTIFICATIONS_CHASER_TIMEOUT_INCREASING_THRESHOLD = 10;
            public const int PENDING_NOTIFICATIONS_CHASER_MAX_TIMEOUT_IN_MINUTES = 15;

            // Max lease timeout for blob. (See: https://msdn.microsoft.com/en-us/library/azure/ee691972.aspx)
            public const int PENDING_NOTIFICATIONS_CHASER_EXCLUSIVE_ACCESS_LOCK_TIMEOUT_IN_MINUTES = 1;
        }

        public class StorageEntities
        {
            public const string EVENT_JOURNAL_TABLE_NAME = "EventJournal";
            public const string EVENT_CONSUMER_SESSIONS_BLOB_CONTAINER_NAME = "event-stream-consumer-session";
            public const string NOTIFICATION_QUEUE_NAME = "event-journal-notifications";
            public const string EVENT_STORE_DEPLOYMENT_TABLE_NAME = "EventStoreDeployment";

            public const string PENDING_NOTIFICATIONS_TABLE_NAME = "PendingNotifications";
            public const string PENDING_NOTIFICATIONS_CHASER_EXCLUSIVE_ACCESS_LOCK_BLOB_CONTAINER_NAME = "pending-notifications-processing";
            public const string PENDING_NOTIFICATIONS_CHASER_EXCLUSIVE_ACCESS_LOCK_BLOB_NAME = "current-lock";

            public class MetadataTable
            {
                public const string EVENT_STREAM_CONSUMERS_IDS_PK = "event-stream-consumer-ids";
            }

            public class MetadataTableProperties
            {
                public const string EVENT_STREAM_READER_ID = "EventStreamReaderId";
                public const string EVENT_STREAM_CONSUMER_NAME = "EventStreamConsumerName";
            }
        }

        public const string DEFAULT_STREAM_READER_NAME = "DEFAULT";

        public static class MetadataProperties
        {
            public const string SESSION_EXPIRES_ON = "SessionExpiresOn";
        }
    }
}
