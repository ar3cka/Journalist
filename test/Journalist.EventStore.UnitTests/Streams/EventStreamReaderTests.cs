using System;
using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamReaderTests
    {
        [Theory, EventStreamReaderData(emptyCursor: true)]
        public async Task ReadEventsAsync_WhenCursorIsEmpty_Throw(
            EventStreamReader reader)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(reader.ReadEventsAsync);
        }

        [Theory, EventStreamReaderData(emptyCursor: false)]
        public async Task ReadEventsAsync_WhenCursorIsNotEmpty_DoesNotThrow(
            EventStreamReader reader)
        {
            await reader.ReadEventsAsync();
        }

        [Theory, EventStreamReaderData(emptyCursor: false)]
        public async Task ReadEventsAsync_WhenCursorIsNotEmpty_ReturnsNotEmptyCollectionEvents(
            EventStreamReader reader)
        {
            await reader.ReadEventsAsync();

            Assert.NotEmpty(reader.Events);
        }

        [Theory, EventStreamReaderData(emptyCursor: true)]
        public void HasEvents_WhenCursorIsEmpty_ReturnsFalse(
            EventStreamReader reader)
        {
            Assert.False(reader.HasEvents);
        }

        [Theory, EventStreamReaderData(emptyCursor: false)]
        public void HaEvents_WhenCursorIsNotEmpty_ReturnsTrue(
            EventStreamReader reader)
        {
            Assert.True(reader.HasEvents);
        }

        [Theory, EventStreamReaderData(emptyCursor: false)]
        public async Task ContinueAsync_WhenCursorIsNotCompleted_Throws(
            EventStreamReader reader)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(reader.ContinueAsync);
        }

        [Theory, EventStreamReaderData(emptyCursor: true)]
        public async Task ContinueAsync_WhenCursorIsCompleted_DoesNotThrow(
            EventStreamReader reader)
        {
            await reader.ContinueAsync();
        }

        [Theory, EventStreamReaderData(emptyCursor: true)]
        public void IsCompleted_WhenCursorIsCompleted_ReturnsTrue(
            EventStreamReader reader)
        {
            Assert.True(reader.IsCompleted);
        }

        [Theory, EventStreamReaderData(emptyCursor: false)]
        public void IsCompleted_WhenCursorIsNotCompleted_ReturnsFalse(
            EventStreamReader reader)
        {
            Assert.False(reader.IsCompleted);
        }
    }
}
