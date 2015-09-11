using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.Extensions;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public sealed class InitialCursorState : CursorState
    {
        private readonly StreamVersion m_version;
        private readonly FetchEvents m_fetch;

        public InitialCursorState(EventStreamHeader streamHeader, StreamVersion fromVersion, FetchEvents fetch)
            : base(streamHeader)
        {
            Require.NotNull(fetch, "fetch");

            m_version = fromVersion;
            m_fetch = fetch;
        }

        public override async Task<EventStreamSlice> FetchSlice()
        {
            var fetchResult = await m_fetch(m_version);
            var slice = new EventStreamSlice(fetchResult.Events);
            if (fetchResult.Events.IsEmpty())
            {
                NextState = new EndOfStreamCursorState(StreamHeader);
            }
            else
            {
                NextState = new FetchingCursorState(fetchResult.StreamHeader, slice.ToStreamVersion, m_fetch);
            }

            return slice;
        }
    }
}
