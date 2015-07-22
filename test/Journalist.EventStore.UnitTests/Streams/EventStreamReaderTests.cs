using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
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

        [Theory, EventStreamReaderData]
        public async Task ReadEventsAsync_EnsureConnectivityStateIsActive(
            [Frozen] Mock<IEventStoreConnectionState> stateMock,
            EventStreamReader reader)
        {
            await reader.ReadEventsAsync();

            stateMock.Verify(self => self.EnsureConnectionIsActive());
        }

        [Theory, EventStreamReaderData]
        public async Task ReadEventsAsync_UseMutationPipelineForEachReceivedEvent(
            [Frozen] Mock<IEventMutationPipeline> pipelineMock,
            [Frozen] SortedList<StreamVersion, JournaledEvent> receivedEvents,
            EventStreamReader reader)
        {
            await reader.ReadEventsAsync();

            pipelineMock.Verify(
                self => self.Mutate(It.Is<JournaledEvent>(e => receivedEvents.ContainsValue(e))),
                Times.Exactly(receivedEvents.Count));
        }

        [Theory, EventStreamReaderData]
        public async Task ReadEventsAsync_ReplaceReceivedEventsWithTheyMutatedVersion(
            [Frozen] Mock<IEventMutationPipeline> pipelineMock,
            JournaledEvent[] mutatedEvents,
            EventStreamReader reader)
        {
            var callsCount = 0;
            pipelineMock
                .Setup(self => self.Mutate(It.IsAny<JournaledEvent>()))
                .Returns(() => mutatedEvents[callsCount++]);

            await reader.ReadEventsAsync();

            Assert.Equal(mutatedEvents.Select(e => e.EventId), reader.Events.Select(e => e.EventId));
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
        public async Task ContinueAsync_EnsureConnectivityStateIsActive(
            [Frozen] Mock<IEventStoreConnectionState> stateMock,
            EventStreamReader reader)
        {
            await reader.ContinueAsync();

            stateMock.Verify(self => self.EnsureConnectionIsActive());
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
