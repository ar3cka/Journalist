using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Journalist.EventStore.Events;
using Journalist.Extensions;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class EventStreamSlice : IEventStreamSlice
    {
        public static readonly EventStreamSlice Empty = new EventStreamSlice();

        private readonly bool m_endOfStream;
        private readonly StreamVersion m_streamVersion;
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;
        private readonly StreamVersion m_sliceStreamVersion;

        public EventStreamSlice(
            StreamVersion streamVersion,
            SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_streamVersion = streamVersion;
            m_events = events;

            if (m_events.IsEmpty())
            {
                m_endOfStream = true;
                m_sliceStreamVersion = streamVersion;
            }
            else
            {
                var lastFetchedVersion = events.Keys[events.Count - 1];
                m_endOfStream = lastFetchedVersion >= streamVersion;
                m_sliceStreamVersion = lastFetchedVersion;
            }
        }

        private EventStreamSlice()
        {
            m_streamVersion = StreamVersion.Unknown;
            m_events = new SortedList<StreamVersion, JournaledEvent>(0);
            m_endOfStream = true;
            m_sliceStreamVersion = StreamVersion.Unknown;
        }

        public IEnumerator<JournaledEvent> GetEnumerator()
        {
            //////////////////////////////////////////////////////////////
            //
            // Because during the fetch, stream can go ahead, that's why
            // we are limiting events with current stream position.
            //
            //////////////////////////////////////////////////////////////
            return m_events
                .Where(pair => pair.Key <= m_streamVersion)
                .Select(journaledEvent => journaledEvent.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public StreamVersion SliceSteamVersion
        {
            get { return m_sliceStreamVersion; }
        }

        public bool EndOfStream
        {
            get { return m_endOfStream; }
        }

        public int Count
        {
            get { return m_events.Count; }
        }
    }
}
