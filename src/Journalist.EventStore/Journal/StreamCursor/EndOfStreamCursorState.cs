using System;
using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.StreamCursor
{
    public class EndOfStreamCursorState : CursorState
    {
        public EndOfStreamCursorState(EventStreamHeader streamHeader) : base(streamHeader)
        {
        }

        public override Task<EventStreamSlice> FetchSlice()
        {
            throw new InvalidOperationException("Cursor is on end of event stream.");
        }
    }
}
