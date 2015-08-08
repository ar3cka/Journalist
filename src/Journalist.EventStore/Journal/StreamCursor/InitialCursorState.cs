using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public sealed class InitialCursorState : CursorState
    {
        private readonly StreamVersion m_version;
        private readonly FetchEvents m_fetch;

        public InitialCursorState(StreamVersion fromVersion, FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            m_version = fromVersion;
            m_fetch = fetch;
        }

        public override async Task<EventStreamSlice> FetchSlice()
        {
            var fetchResult = await m_fetch(m_version);
            var slice = new EventStreamSlice(fetchResult.SteamVersion, fetchResult.Events);
            if (slice.EndOfStream)
            {
                NextState = new EndOfStreamCursorState();
            }
            else
            {
                NextState = new FetchingCursorState(slice.SliceSteamVersion, m_fetch);
            }

            return slice;
        }
    }
}
