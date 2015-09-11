using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public abstract class CursorState
    {
        private readonly EventStreamHeader m_streamHeader;

        protected CursorState(EventStreamHeader streamHeader)
        {
            m_streamHeader = streamHeader;
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

        public EventStreamHeader StreamHeader
        {
            get { return m_streamHeader; }
        }

        public CursorState NextState { get; protected set; }

    }
}
