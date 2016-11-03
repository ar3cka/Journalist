using System.Threading.Tasks;
using Journalist.EventStore.Events;

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

            Ensure.True(slice.ToStreamVersion <= StreamHeader.Version, "slice.ToStreamVersion <= StreamHeader.Version");
            if (StreamHeader.Version == slice.ToStreamVersion)
            {
                NextState = new EndOfStreamCursorState(StreamHeader);
            }
            else
            {
                NextState = new FetchingCursorState(StreamHeader, slice.ToStreamVersion.Increment(), m_fetch);
            }

            return slice;
        }
    }
}
