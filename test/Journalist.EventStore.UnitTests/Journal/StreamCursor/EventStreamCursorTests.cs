using Journalist.EventStore.Journal.StreamCursor;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal.StreamCursor
{
    public class EventStreamCursorTests
    {
        [Fact]
        public void EndOfStream_ForUninitializedStreamCursor_ReturnsTrue()
        {
            Assert.True(EventStreamCursor.UninitializedStream.EndOfStream);
        }

        [Fact]
        public void EqualsOperator_ForTwoUninitializedStreamCursors_ReturnsTrue()
        {
            var cursor1 = EventStreamCursor.UninitializedStream;
            var cursor2 = EventStreamCursor.UninitializedStream;

            Assert.Equal(cursor1, cursor2);
            Assert.True(cursor1 == cursor2);
        }
    }
}
