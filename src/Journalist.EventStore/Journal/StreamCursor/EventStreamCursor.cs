using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class EventStreamCursor : IEventStreamCursor
    {
        public static readonly EventStreamCursor UninitializedStream = new EventStreamCursor();

        private readonly StreamVersion m_cursorStreamVersion;
        private EventStreamSlice m_slice;
        private CursorState m_state;

        private EventStreamCursor(EventStreamHeader streamHeader, StreamVersion fromVersion, FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            m_state = new InitialCursorState(streamHeader, fromVersion, fetch);
            m_slice = EventStreamSlice.Empty;
            m_cursorStreamVersion = fromVersion;
        }

        private EventStreamCursor(EventStreamHeader streamHeader, StreamVersion fromVersion)
        {
            m_state = new EndOfStreamCursorState(streamHeader);
            m_slice = EventStreamSlice.Empty;
            m_cursorStreamVersion = fromVersion;
        }

        public static IEventStreamCursor CreateActiveCursor(EventStreamHeader streamHeader, StreamVersion fromVersion, FetchEvents fetch)
        {
            return new EventStreamCursor(streamHeader, fromVersion, fetch);
        }

        public static IEventStreamCursor CreateEmptyCursor(EventStreamHeader streamHeader, StreamVersion fromVersion)
        {
            return new EventStreamCursor(streamHeader, fromVersion);
        }

        private EventStreamCursor()
        {
            m_state = new EndOfStreamCursorState(EventStreamHeader.Unknown);
            m_slice = EventStreamSlice.Empty;
        }

        public async Task FetchSlice()
        {
            m_slice = await m_state.FetchSlice();
            m_state = m_state.NextState;
        }

        private void AssertCursorWasInitialized()
        {
            if (CursorState.IsInitialState(m_state))
            {
                throw new InvalidOperationException("Stream cursor in initial state.");
            }
        }

        public IEventStreamSlice Slice
        {
            get
            {
                AssertCursorWasInitialized();

                return m_slice;
            }
        }

        public EventStreamHeader StreamHeader
        {
            get
            {
                return m_state.StreamHeader;
            }
        }

        public StreamVersion StreamVersion
        {
            get
            {
                return m_state.StreamHeader.Version;
            }
        }

        public StreamVersion CursorStreamVersion
        {
            get
            {
                return m_cursorStreamVersion;
            }
        }

        public bool EndOfStream
        {
            get
            {
                return CursorState.IsEndOfStream(m_state);
            }
        }
    }
}
