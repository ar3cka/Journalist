namespace Journalist.EventStore
{
    internal class Constants
    {
        public class Settings
        {
            public const int DEFAULT_EVENT_SLICE_SIZE = 100;
        }

        public class StorageEntities
        {
            public const string EVENT_JOURNAL_TABLE_NAME = "EventJournal";
            public const string EVENT_STREAM_CONSUMER_SESSIONS_BLOB_NAME = "event-stream-consumer-session";
        }

        public const string DEFAULT_STREAM_READER_NAME = "DEFAULT";
    }
}
