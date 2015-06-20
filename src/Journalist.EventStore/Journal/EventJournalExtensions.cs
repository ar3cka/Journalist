using System.Threading.Tasks;
using Journalist.EventStore.Journal.StreamCursor;

namespace Journalist.EventStore.Journal
{
    public static class EventJournalExtensions
    {
        public static Task<EventStreamCursor> OpenEventStreamCursorAsync(
            this IEventJournal journal,
            string streamName,
            StreamVersion fromVersion)
        {
            Require.NotNull(journal, "journal");

            return journal.OpenEventStreamCursorAsync(
                streamName,
                fromVersion,
                Constants.Settings.DEFAULT_EVENT_SLICE_SIZE);
        }

        public static Task<EventStreamCursor> OpenEventStreamCursorAsync(
            this IEventJournal journal,
            string streamName,
            StreamVersion fromVersion,
            StreamVersion toVersion)
        {
            Require.NotNull(journal, "journal");

            return journal.OpenEventStreamCursorAsync(
                streamName,
                fromVersion,
                toVersion,
                Constants.Settings.DEFAULT_EVENT_SLICE_SIZE);
        }

        public static Task<EventStreamCursor> OpenEventStreamCursorAsync(
            this IEventJournal journal,
            string streamName)
        {
            Require.NotNull(journal, "journal");

            return journal.OpenEventStreamCursorAsync(
                streamName,
                Constants.Settings.DEFAULT_EVENT_SLICE_SIZE);
        }
    }
}