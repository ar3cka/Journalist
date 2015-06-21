using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public delegate Task<SortedList<StreamVersion, JournaledEvent>> FetchEvents(StreamVersion fromVersion);

    public class EventStreamCursor
    {
        public static readonly EventStreamCursor Empty = new EventStreamCursor(
            EventStreamPosition.Start,
            StreamVersion.Unknown,
            from => new SortedList<StreamVersion, JournaledEvent>(0).YieldTask());

        private EventStreamSlice m_slice;
        private CursorState m_state;

        public EventStreamCursor(EventStreamPosition position, StreamVersion fromVersion, FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            if (EventStreamPosition.IsAtStart(position))
            {
                m_state = new EndOfStreamCursorState();
                m_slice = EventStreamSlice.Empty;
            }
            else
            {
                m_state = new InitialCursorState(position, fromVersion, fetch);
            }
        }

        public async Task FetchSlice()
        {
            m_slice = await m_state.FetchSlice();
            m_state = m_state.NextState;
        }

        public EventStreamSlice Slice
        {
            get
            {
                if (CursorState.IsInitialState(m_state))
                {
                    throw new InvalidOperationException("Stream cursor in initial state.");
                }

                return m_slice;
            }
        }

        public StreamVersion CurrentVersion
        {
            get { return m_slice.SlicePosition.Version; }
        }

        public bool EndOfStream
        {
            get { return CursorState.IsEndOfStream(m_state); }
        }
    }
}
