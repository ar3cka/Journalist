using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class FetchingCursorState : CursorState
    {
        private readonly StreamVersion m_sliceSteamVersion;
        private readonly FetchEvents m_fetch;

        public FetchingCursorState(
            StreamVersion sliceSteamVersion,
            FetchEvents fetch)
        {
            Require.NotNull(fetch, "fetch");

            m_sliceSteamVersion = sliceSteamVersion;
            m_fetch = fetch;
        }

        public override async Task<EventStreamSlice> FetchSlice()
        {
            var fetchResult = await m_fetch(m_sliceSteamVersion.Increment(1));
            var slice = new EventStreamSlice(fetchResult.SteamVersion, fetchResult.Events);
            if (slice.EndOfStream)
            {
                NextState = new EndOfStreamCursorState();
            }
            else
            {
                NextState = new FetchingCursorState(
                    slice.SliceSteamVersion,
                    m_fetch);
            }

            return slice;
        }
    }
}
