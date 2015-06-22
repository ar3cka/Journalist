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
    }
}
