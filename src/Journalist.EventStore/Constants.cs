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
            public const int NOTIFICATION_RETRY_DELIVERY_TIMEOUT_MULTIPLYER_SEC = 2;
            public const int NOTIFICATION_QUEUE_PARTITION_COUNT = 32;
        }

        public class StorageEntities
        {
            public const string EVENT_JOURNAL_TABLE_NAME = "EventJournal";
            public const string EVENT_STREAM_CONSUMER_SESSIONS_BLOB_NAME = "event-stream-consumer-session";
            public const string NOTIFICATION_QUEUE_NAME = "event-journal-notifications";
            public const string EVENT_STORE_DEPLOYMENT_TABLE_NAME = "EventStoreDeployment";

            public class MetadataTable
            {
                public const string EVENT_STREAM_CONSUMERS_IDS_PK = "event-stream-consumer-ids";
                public const string EVENT_STREAM_READERS_IDS_PK = "event-stream-reader-ids";
            }

            public class MetadataTableProperties
            {
                public const string EVENT_STREAM_CONSUMER_ID = "EventStreamConsumerId";
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
