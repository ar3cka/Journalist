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
        [EventStreamReaderCustomization(hasEvents: false)]
        public async Task ReceiveAsync_WhenReaderIsEmpty_ReturnsEmptyStreamCode(
            EventStreamConsumer consumer)
        {
            Assert.Equal(ReceivingResultCode.EmptyStream, await consumer.ReceiveEventsAsync());
        }

        [Theory]
        [EventStreamReaderCustomization(hasEvents: false)]
        public async Task ReceiveAsync_WhenReaderIsEmpty_FreesSession(
            [Frozen] Mock<IEventStreamConsumingSession> sessionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            sessionMock.Verify(self => self.FreeAsync(), Times.Once());
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task ReceiveAsync_WhenReaderIsNotEmpty_ReturnsEventsReceivedCode(
            EventStreamConsumer consumer)
        {
            Assert.Equal(ReceivingResultCode.EventsReceived, await consumer.ReceiveEventsAsync());
        }

        [Theory]
        [EventStreamReaderCustomization(leaderPromotion: false)]
        public async Task ReceiveAsync_WhenReaderWasNotBeenPromotedToLeader_ReturnsPromotionFailedCode(
            EventStreamConsumer consumer)
        {
            Assert.Equal(ReceivingResultCode.PromotionFailed, await consumer.ReceiveEventsAsync());
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task ReceiveAsync_WhenReaderIsNotEmpty_ReadEventsFromReader(
            [Frozen] Mock<IEventStreamReader> readerMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            readerMock.Verify(self => self.ReadEventsAsync(), Times.Once());
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task EnumerateEvents_WhenReceiveMethodWasCalled_ReturnsEventsFromReader(
            [Frozen] IReadOnlyList<JournaledEvent> events,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var receivedEvents = consumer.EnumerateEvents();

            Assert.Equal(events, receivedEvents);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public void EnumerateEvents_WhenReceiveMethodWasNotCalled_Throws(
            EventStreamConsumer consumer)
        {
            Assert.Throws<InvalidOperationException>(() => consumer.EnumerateEvents().ToList());
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task ReceiveEventsAsync_WhenReceivingWasStarted_CommitsReaderVersion(
            [Frozen] StreamVersion streamVersion,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync(); // starts receiving
            await consumer.ReceiveEventsAsync(); // continues receiving and commits previous events

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(reader.StreamVersion.Increment(reader.Events.Count), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(disableAutoCommit: true)]
        public async Task ReceiveEventsAsync_WhenReceivingWasStartedAndAutoCommitDisabled_DoesNotCommitReaderVersion(
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();
            await consumer.ReceiveEventsAsync();

            Assert.Equal(0, commitStreamVersionMock.CallsCount);
            Assert.Equal(StreamVersion.Unknown, commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(disableAutoCommit: true)]
        public async Task CommitProcessedStreamVersionAsync_WhenReceivingWasStartedAndAutoCommitDisabled_DoesNotCommitReaderVersion(
            [Frozen] StreamVersion streamVersion,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            var expectedVersion = reader.StreamVersion.Increment(reader.Events.Count * 2);

            await consumer.ReceiveEventsAsync();
            await consumer.ReceiveEventsAsync();

            await consumer.CommitProcessedStreamVersionAsync(false);

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(expectedVersion, commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
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
        [EventStreamReaderCustomization]
        public async Task CloseAsync_WhenReaderHasUnprocessedEventsAndNoMessagesWereConsumed_DoesNotThrow(
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();
            await consumer.CloseAsync();

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task CloseAsync_FreesConsumingSessions(
            [Frozen] Mock<IEventStreamConsumingSession> sessionMock,
            EventStreamConsumer consumer)
        {
            await consumer.CloseAsync();

            sessionMock.Verify(self => self.FreeAsync(), Times.Once());
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task CloseAsync_WhenReaderHasUnprocessedEventsAndOnMessagesWasConsumed_CommitsConsumedVersion(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var handledEventCount = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                if (handledEventCount >= 1)
                {
                    break;
                }

                handledEventCount++;
            }

            await consumer.CloseAsync();

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(version.Increment(), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task CloseAsync_WhenReaderHasUnprocessedEventsAndCurrentEventHasBeenCommited_SkipCommit(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            foreach (var e in consumer.EnumerateEvents())
            {
                await consumer.CommitProcessedStreamVersionAsync();
                break;
            }

            await consumer.CloseAsync();

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(version.Increment(), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task CloseAsync_WhenReaderHasNoUnprocessedEvents_NotThrows(
            EventStreamConsumer consumer)
        {
            await consumer.CloseAsync();
        }

        [Theory]
        [EventStreamReaderCustomization(completed: true)]
        public async Task CloseAsync_WhenReaderInCompletedStateAndHasNoUnprocessedEvents_CommitsConsumedVersion(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] IEventStreamReader reader,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();
            consumer.EnumerateEvents().ToList();

            await consumer.CloseAsync();

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(reader.StreamVersion.Increment(reader.Events.Count), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
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
                await consumer.CommitProcessedStreamVersionAsync();
                break;
            }


            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(version.Increment(handledEvents), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization(disableAutoCommit: true)]
        public async Task RememberConsumedStreamVersionAsync_WhenEventWasConsumedAndAutoCommitDisabled_CommitsConsumedVersion(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
                await consumer.CommitProcessedStreamVersionAsync();
                break;
            }

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(version.Increment(handledEvents), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task RememberConsumedStreamVersionAsync_WhenAllEventsWereConsumed_CommitsReaderVersion(
            [Frozen] StreamVersion streamVersion,
            [Frozen] IEventStreamReader streamReader,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
            }

            await consumer.CommitProcessedStreamVersionAsync(true);

            Assert.Equal(1, commitStreamVersionMock.CallsCount);
            Assert.Equal(streamReader.StreamVersion.Increment(streamReader.Events.Count), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task RememberConsumedStreamVersionAsync_WhenSkipCurrentFlagIsTrue_CommitsOnlyProcessedEvents(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            EventStreamConsumer consumer)
        {
            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
                await consumer.CommitProcessedStreamVersionAsync(skipCurrent: true);
                break;
            }

            Assert.Equal(0, commitStreamVersionMock.CallsCount);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task RememberConsumedStreamVersionAsync_WhenSkipCurrentFlagIsTrue_CommitsOnlyProcessedEvents(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] Mock<IEventStreamReader> readerMock,
            [Frozen] JournaledEvent[] events,
            EventStreamConsumer consumer)
        {
            var streamVersion = version.Increment(events.Length);

            readerMock
                .Setup(self => self.StreamVersion)
                .Returns(streamVersion);

            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
                await consumer.CommitProcessedStreamVersionAsync(skipCurrent: true);
            }

            Assert.Equal(handledEvents - 1, commitStreamVersionMock.CallsCount);
            Assert.Equal(streamVersion.Decrement(), commitStreamVersionMock.CommitedVersion);
        }

        [Theory]
        [EventStreamReaderCustomization]
        public async Task RememberConsumedStreamVersionAsync_WhenLatestManuallyCommitedVersionEqualsToStreamCurrent_SkipCommitOnReceiveAsync(
            [Frozen] StreamVersion version,
            [Frozen] CommitStreamVersionFMock commitStreamVersionMock,
            [Frozen] Mock<IEventStreamReader> readerMock,
            [Frozen] JournaledEvent[] events,
            EventStreamConsumer consumer)
        {
            var streamVersion = version.Increment(events.Length);

            readerMock
                .Setup(self => self.StreamVersion)
                .Returns(streamVersion);

            await consumer.ReceiveEventsAsync();

            var handledEvents = 0;
            foreach (var e in consumer.EnumerateEvents())
            {
                handledEvents++;
                await consumer.CommitProcessedStreamVersionAsync();
            }

            await consumer.ReceiveEventsAsync();

            Assert.Equal(handledEvents, commitStreamVersionMock.CallsCount);
            Assert.Equal(streamVersion, commitStreamVersionMock.CommitedVersion);
        }
    }
}
