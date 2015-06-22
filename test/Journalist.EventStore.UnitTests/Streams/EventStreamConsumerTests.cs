using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamConsumerTests
    {
        [Theory]
        [EventStreamReaderCustomization(HasEvents = false)]
        public async Task ReceiveAsync_WhenReaderIsEmpty_ReturnsFalse(
            EventStreamConsumer consumer)
        {
            Assert.False(await consumer.ReceiveEventsAsync());
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task ReceiveAsync_WhenReaderIsNotEmpty_ReturnsTrue(
            EventStreamConsumer consumer)
        {
            Assert.True(await consumer.ReceiveEventsAsync());
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task ReceiveAsync_WhenReaderIsNotEmpty_ReadEventsFromReader(
            [Frozen] Mock<IEventStreamReader> readerMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            readerMock.Verify(self => self.ReadEventsAsync(), Times.Once());
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task EnumerateEvents_WhenReceiveMethodWasCalled_ReturnsEventsFromReader(
            [Frozen] IReadOnlyList<JournaledEvent> events,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var receivedEvents = consumer.EnumerateEvents();

            Assert.Equal(events, receivedEvents);
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public void EnumerateEvents_WhenReceiveMethodWasNotCalled_Throws(
            EventStreamConsumer consumer)
        {
            Assert.Throws<InvalidOperationException>(() => consumer.EnumerateEvents().ToList());
        }

        [Theory]
        [EventStreamReaderCustomization(Completed = true)]
        public async Task ReceiveEventsAsync_WhenReaderCompleted_ContinueReading(
            [Frozen] Mock<IEventStreamReader> readerMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            readerMock.Verify(self => self.ContinueAsync(), Times.Once());
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task ReceiveEventsAsync_WhenReceivingWasStarted_CommitsReaderVersion(
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync(); // starts receiving
            await consumer.ReceiveEventsAsync(); // continues receiving

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(reader.CurrentStreamVersion, commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task ReceiveEventsAsync_WhenReceivingWasNotStartedBefore_CommitsReaderVersion(
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            Assert.Equal(0, commitStreamVersionMock.CallsCount);
            Assert.Equal(StreamVersion.Unknown, commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task CloseAsync_WhenReaderHasUnprocessedEvents_Throws(
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(consumer.CloseAsync);
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true)]
        public async Task CloseAsync_WhenReaderHasNoUnprocessedEvents_NotThrows(
            EventStreamConsumer consumer)
        {
            await consumer.CloseAsync();
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true, Completed = true)]
        public async Task CloseAsync_WhenReaderInCompletedStateAndHasNoUnprocessedEvents_CommitsConsumedVersion(
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();
            consumer.EnumerateEvents().ToList(); // drains events from reader;

            await consumer.CloseAsync();

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(reader.CurrentStreamVersion, commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true, Completed = true)]
        public async Task RememberConsumedStreamVersionAsync_WhenEventWasConsumed_CommitsConsumedVersion(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
                await consumer.RememberConsumedStreamVersionAsync();
                break;
            }


            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(version.Increment(handledEvents), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(HasEvents = true, Completed = true)]
        public async Task RememberConsumedStreamVersionAsync_WhenLatestManuallyCommitedVersionEqualsToStreamCurrent_SkipCommitOnRecevieAsync(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] Mock<IEventStreamReader> readerMock,
            [Frozen] JournaledEvent[] events,
            EventStreamConsumer consumer)
        {
            var streamVersion = version.Increment(events.Length);

            readerMock
                .Setup(self => self.CurrentStreamVersion)
                .Returns(streamVersion);

            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
                await consumer.RememberConsumedStreamVersionAsync();
            }

            await consumer.ReceiveEventsAsync();

            Assert.Equal(handledEvents, commitStreamVersionMock.CallsCount);
            Assert.Equal(streamVersion, commitStreamVersionMock.CommitedVersion);
        }
    }
}
