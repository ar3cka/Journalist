using Journalist.EventStore.Journal.StreamCursor;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal.StreamCursor
{
    public class EventStreamCursorTests
    {
        [Fact]
        public void EndOfStream_ForEmptyCursor_ReturnsTrue()
        {
            Assert.True(EventStreamCursor.Empty.EndOfStream);
        }

        [Fact]
        public void EqualsOperator_ForTwoEmptyCursors_ReturnsTrue()
        {
            var cursor1 = EventStreamCursor.Empty;
            var cursor2 = EventStreamCursor.Empty;

            Assert.Equal(cursor1, cursor2);
            Assert.True(cursor1 == cursor2);
        }

        [Fact]
        public void EndOfSteam_WhenCursorIsEmpty_ReturnsTrue()
        {
            Assert.True(EventStreamCursor.Empty.EndOfStream);
        }
    }
}
