using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public delegate Task<FetchEventsResult> FetchEvents(StreamVersion fromVersion);

    public class FetchEventsResult
    {
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;
        private readonly StreamVersion m_currentStreamVersion;

        public FetchEventsResult(StreamVersion currentStreamVersion, SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_events = events;
            m_currentStreamVersion = currentStreamVersion;
        }

        public StreamVersion CurrentStreamVersion => m_currentStreamVersion;

        public SortedList<StreamVersion, JournaledEvent> Events => m_events;
    }
}
