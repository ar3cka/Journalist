using System;
using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamReaderTests
    {
        [Theory, EmptyEventStreamCursor]
        public async Task ReadEventsAsync_WhenCursorIsEmpty_Throw(EventStreamReader reader)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(reader.ReadEventsAsync);
        }

        [Theory, NotEmptyEventStreamCursor]
        public async Task ReadEventsAsync_WhenCursorIsNotEmpty_DoesNotThrow(EventStreamReader reader)
        {
            await reader.ReadEventsAsync();
        }

        [Theory, NotEmptyEventStreamCursor]
        public async Task ReadEventsAsync_WhenCursorIsNotEmpty_ReturnsNotEmptyCollectionEvents(EventStreamReader reader)
        {
            await reader.ReadEventsAsync();

            Assert.NotEmpty(reader.Events);
        }

        [Theory, EmptyEventStreamCursor]
        public void HasMoreEvents_WhenCursorIsEmpty_ReturnsFalse(EventStreamReader reader)
        {
            Assert.False(reader.HasMoreEvents);
        }

        [Theory, NotEmptyEventStreamCursor]
        public void HasMoreEvents_WhenCursorIsNotEmpty_ReturnsTrue(EventStreamReader reader)
        {
            Assert.True(reader.HasMoreEvents);
        }
    }
}
