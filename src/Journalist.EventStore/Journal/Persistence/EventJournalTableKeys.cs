using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.Persistence
{
    public static class EventJournalTableKeys
    {
        public static readonly string Header = "HEAD";
        public static readonly string PendingNotificationPrefix = "PNDNTF|";

        public static string GetPendingNotificationRowKey(StreamVersion streamVersion)
        {
            return PendingNotificationPrefix + streamVersion;

        }
    }
}
