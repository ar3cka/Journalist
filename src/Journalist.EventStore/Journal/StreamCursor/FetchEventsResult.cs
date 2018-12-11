using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public delegate Task<FetchEventsResult> FetchEvents(StreamVersion fromVersion);

    public class FetchEventsResult
    {
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;
        private readonly bool m_isFetchingCompleted;

        public FetchEventsResult(bool isFetchingCompleted, SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_events = events;
            m_isFetchingCompleted = isFetchingCompleted;
        }

        public bool IsFetchingCompleted => m_isFetchingCompleted;

        public SortedList<StreamVersion, JournaledEvent> Events => m_events;
    }
}
