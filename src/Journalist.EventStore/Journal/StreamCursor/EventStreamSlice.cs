using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class EventStreamSlice : IReadOnlyCollection<JournaledEvent>
    {
        public static readonly EventStreamSlice Empty = new EventStreamSlice();

        private readonly bool m_endOfStream;
        private readonly EventStreamPosition m_streamPosition;
        private readonly SortedList<StreamVersion, JournaledEvent> m_events;
        private readonly EventStreamPosition m_currentSlicePosition;

        public EventStreamSlice(EventStreamPosition streamPosition, SortedList<StreamVersion, JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            m_streamPosition = streamPosition;
            m_events = events;

            var lastFetchedVersion = events.Keys[events.Count - 1];
            m_endOfStream = lastFetchedVersion >= streamPosition.Version;
            m_currentSlicePosition = new EventStreamPosition(streamPosition.ETag, lastFetchedVersion);
        }

        private EventStreamSlice()
        {
            m_streamPosition = EventStreamPosition.Start;
            m_events = new SortedList<StreamVersion, JournaledEvent>(0);
            m_endOfStream = true;
            m_currentSlicePosition = EventStreamPosition.Start;
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
                .Where(pair => pair.Key <= m_streamPosition.Version)
                .Select(journaledEvent => journaledEvent.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public EventStreamPosition SlicePosition
        {
            get { return m_currentSlicePosition; }
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
