using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamProducerTests
    {
        [Theory]
        [AutoMoqData]
        public async Task PublishAsync_AppendsEventsToStreamUsingStreamWriter(
            [Frozen] Mock<IEventStreamWriter> writerMock,
            JournaledEvent[] events,
            EventStreamProducer producer)
        {
            await producer.PublishAsync(events);

            writerMock.Verify(self => self.AppendEventsAsync(events));
        }

        [Theory]
        [AutoMoqData]
        public async Task PublishAsync_WhenWriterThrows_MovesItToTheEndOfStreamAndTriesAppendAgain(
            [Frozen] Mock<IEventStreamWriter> writerMock,
            JournaledEvent[] events,
            EventStreamProducer producer)
        {
            writerMock
                .Setup(self => self.AppendEventsAsync(events))
                .Throws<EventStreamConcurrencyException>();

            await Assert.ThrowsAsync<EventStreamConcurrencyException>(() => producer.PublishAsync(events));

            writerMock.Verify(self => self.MoveToEndOfStreamAsync());
            writerMock.Verify(self => self.AppendEventsAsync(events), Times.Exactly(2));
        }
    }
}
