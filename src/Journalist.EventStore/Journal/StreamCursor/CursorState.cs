using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public abstract class CursorState
    {
        private readonly EventStreamPosition m_streamPosition;

        protected CursorState(EventStreamPosition streamPosition)
        {
            m_streamPosition = streamPosition;
        }

        public static bool IsEndOfStream(CursorState state)
        {
            Require.NotNull(state, "state");

            return state is EndOfStreamCursorState;
        }

        public static bool IsInitialState(CursorState state)
        {
            Require.NotNull(state, "state");

            return state is InitialCursorState;
        }

        public static bool IsFetching(CursorState state)
        {
            Require.NotNull(state, "state");

            return state is FetchingCursorState;
        }

        public abstract Task<EventStreamSlice> FetchSlice();

        public EventStreamPosition StreamPosition
        {
            get { return m_streamPosition; }
        }

        public CursorState NextState { get; protected set; }

    }
}
