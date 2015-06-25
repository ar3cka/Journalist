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
        public async Task ReadEventsAsync_WhenCursorIsEmpty_Throw(
            EventStreamReader reader)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(reader.ReadEventsAsync);
        }

        [Theory, NotEmptyEventStreamCursor]
        public async Task ReadEventsAsync_WhenCursorIsNotEmpty_DoesNotThrow(
            EventStreamReader reader)
        {
            await reader.ReadEventsAsync();
        }

        [Theory, NotEmptyEventStreamCursor]
        public async Task ReadEventsAsync_WhenCursorIsNotEmpty_ReturnsNotEmptyCollectionEvents(
            EventStreamReader reader)
        {
            await reader.ReadEventsAsync();

            Assert.NotEmpty(reader.Events);
        }

        [Theory, EmptyEventStreamCursor]
        public void HasEvents_WhenCursorIsEmpty_ReturnsFalse(
            EventStreamReader reader)
        {
            Assert.False(reader.HasEvents);
        }

        [Theory, NotEmptyEventStreamCursor]
        public void HaEvents_WhenCursorIsNotEmpty_ReturnsTrue(
            EventStreamReader reader)
        {
            Assert.True(reader.HasEvents);
        }

        [Theory, NotEmptyEventStreamCursor]
        public async Task ContinueAsync_WhenCursorIsNotCompleted_Throws(
            EventStreamReader reader)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(reader.ContinueAsync);
        }

        [Theory, EmptyEventStreamCursor]
        public async Task ContinueAsync_WhenCursorIsCompleted_DoesNotThrow(
            EventStreamReader reader)
        {
            await reader.ContinueAsync();
        }

        [Theory, EmptyEventStreamCursor]
        public void IsCompleted_WhenCursorIsCompleted_ReturnsTrue(
            EventStreamReader reader)
        {
            Assert.True(reader.IsCompleted);
        }

        [Theory, NotEmptyEventStreamCursor]
        public void IsCompleted_WhenCursorIsNotCompleted_ReturnsFalse(
            EventStreamReader reader)
        {
            Assert.False(reader.IsCompleted);
        }
    }
}
