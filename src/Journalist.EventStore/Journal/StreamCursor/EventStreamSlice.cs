using System.Collections;
using System.Collections.Generic;
using Journalist.EventStore.Events;
using Journalist.Extensions;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class EventStreamSlice : IEventStreamSlice
    {
        public static readonly EventStreamSlice Empty = new EventStreamSlice();

        private readonly SortedList<StreamVersion, JournaledEvent> m_events;
        private readonly StreamVersion m_fromStreamVersion;
        private readonly StreamVersion m_toStreamVersion;

        public EventStreamSlice(
            SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_events = events;

            if (m_events.IsEmpty())
            {
                m_fromStreamVersion = StreamVersion.Unknown;
                m_toStreamVersion = StreamVersion.Unknown;
            }
            else
            {
                m_fromStreamVersion = events.Keys[0];
                m_toStreamVersion = events.Keys[events.Count - 1];
            }
        }

        private EventStreamSlice()
        {
            m_events = new SortedList<StreamVersion, JournaledEvent>(0);
            m_fromStreamVersion = StreamVersion.Unknown;
            m_toStreamVersion = StreamVersion.Unknown;
        }

        public IEnumerator<JournaledEvent> GetEnumerator()
        {
            return m_events.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public StreamVersion FromStreamVersion
        {
            get { return m_fromStreamVersion; }
        }

        public StreamVersion ToStreamVersion
        {
            get { return m_toStreamVersion; }
        }

        public int Count
        {
            get { return m_events.Count; }
        }
    }
}
