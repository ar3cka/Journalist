using System;
using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class EventStreamCursor : IEventStreamCursor
    {
        public static readonly EventStreamCursor Empty = new EventStreamCursor();

        private EventStreamSlice m_slice;
        private CursorState m_state;

        public EventStreamCursor(StreamVersion fromVersion, FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            m_state = new InitialCursorState(fromVersion, fetch);
            m_slice = EventStreamSlice.Empty;
        }

        private EventStreamCursor()
        {
            m_state = new EndOfStreamCursorState();
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

        public StreamVersion CurrentVersion
        {
            get
            {
                AssertCursorWasInitialized();

                return m_slice.ToStreamVersion;
            }
        }

        public bool EndOfStream
        {
            get { return CursorState.IsEndOfStream(m_state); }
        }
    }
}
