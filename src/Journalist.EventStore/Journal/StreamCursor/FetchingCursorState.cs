using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.Extensions;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class FetchingCursorState : CursorState
    {
        private readonly StreamVersion m_sliceSteamVersion;
        private readonly FetchEvents m_fetch;

        public FetchingCursorState(
            EventStreamPosition streamPosition,
            StreamVersion sliceSteamVersion,
            FetchEvents fetch) : base(streamPosition)
        {
            Require.NotNull(fetch, "fetch");

            m_sliceSteamVersion = sliceSteamVersion;
            m_fetch = fetch;
        }

        public override async Task<EventStreamSlice> FetchSlice()
        {
            var fetchResult = await m_fetch(m_sliceSteamVersion.Increment(1));
            var slice = new EventStreamSlice(fetchResult.Events);
            if (slice.IsEmpty())
            {
                NextState = new EndOfStreamCursorState(StreamPosition);
            }
            else
            {
                NextState = new FetchingCursorState(fetchResult.StreamPosition, slice.ToStreamVersion, m_fetch);
            }

            return slice;
        }
    }
}
