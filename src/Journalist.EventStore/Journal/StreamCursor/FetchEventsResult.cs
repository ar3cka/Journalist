using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public delegate Task<FetchEventsResult> FetchEvents(StreamVersion fromVersion);

    public class FetchEventsResult
    {
        private readonly EventStreamPosition m_streamPosition;
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;

        public FetchEventsResult(EventStreamPosition streamPosition, SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_streamPosition = streamPosition;
            m_events = events;
        }

        public StreamVersion SteamVersion
        {
            get { return m_streamPosition.Version; }
        }

        public EventStreamPosition StreamPosition
        {
            get { return m_streamPosition; }
        }

        public SortedList<StreamVersion, JournaledEvent> Events
        {
            get { return m_events; }
        }
    }
}
