using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public delegate Task<FetchEventsResult> FetchEvents(StreamVersion fromVersion);

    public class FetchEventsResult
    {
        private readonly EventStreamHeader m_streamHeader;
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;

        public FetchEventsResult(EventStreamHeader streamHeader, SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_streamHeader = streamHeader;
            m_events = events;
        }

        public StreamVersion SteamVersion
        {
            get { return m_streamHeader.Version; }
        }

        public EventStreamHeader StreamHeader
        {
            get { return m_streamHeader; }
        }

        public SortedList<StreamVersion, JournaledEvent> Events
        {
            get { return m_events; }
        }
    }
}
