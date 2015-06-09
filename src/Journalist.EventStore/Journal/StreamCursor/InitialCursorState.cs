using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public sealed class InitialCursorState : CursorState
    {
        private readonly EventStreamPosition m_position;
        private readonly StreamVersion m_version;
        private readonly FetchEvents m_fetch;

        public InitialCursorState(EventStreamPosition position, StreamVersion fromVersion, FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            m_position = position;
            m_version = fromVersion;
            m_fetch = fetch;
        }

        public override async Task<EventStreamSlice> FetchSlice()
        {
            var slice = new EventStreamSlice(m_position, await m_fetch(m_version));

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
