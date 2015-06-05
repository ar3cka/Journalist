using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class FetchingCursorState : CursorState
    {
        private readonly EventStreamPosition m_position;
        private readonly EventStreamPosition m_currentSlicePosition;
        private readonly FetchEvents m_fetch;

        public FetchingCursorState(
            EventStreamPosition position,
            EventStreamPosition currentSlicePosition,
            FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            m_position = position;
            m_currentSlicePosition = currentSlicePosition;
            m_fetch = fetch;
        }

        public override async Task<EventStreamSlice> FetchSlice()
        {
            var slice = new EventStreamSlice(m_position, await m_fetch(m_currentSlicePosition.Version.Increment(1)));

            if (slice.EndOfStream)
            {
                NextState = new EndOfStreamCursorState();
            }
            else
            {
                NextState = new FetchingCursorState(m_position, slice.SlicePosition, m_fetch);
            }

            return slice;
        }
    }
}