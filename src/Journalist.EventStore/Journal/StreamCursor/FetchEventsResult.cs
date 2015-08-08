using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public delegate Task<FetchEventsResult> FetchEvents(StreamVersion fromVersion);

    public class FetchEventsResult
    {
        private readonly StreamVersion m_steamVersion;
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;

        public FetchEventsResult(StreamVersion steamVersion, SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_steamVersion = steamVersion;
            m_events = events;
        }

        public StreamVersion SteamVersion
        {
            get { return m_steamVersion; }
        }

        public SortedList<StreamVersion, JournaledEvent> Events
        {
            get { return m_events; }
        }
    }
}
