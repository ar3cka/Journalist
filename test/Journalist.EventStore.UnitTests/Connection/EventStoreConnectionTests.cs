using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Connection
{
    public class EventStoreConnectionTests
    {
        [Theory, AutoMoqData]
        public async Task OpenReaderAsync_ReturnsReaderForSpecifiedEventStream(
            EventStoreConnection eventStoreConnection,
            string streamName)
        {
            var reader = await eventStoreConnection.CreateStreamReaderAsync(streamName);

            Assert.Equal(streamName, reader.StreamName);
        }

        [Theory, AutoMoqData]
        public void Constructor_ChangeConnectionStateToActive(
            [Frozen] Mock<IEventStoreConnectionState> stateMock,
            EventStoreConnection eventStoreConnection,
            string streamName)
        {
            stateMock.Verify(self => self.ChangeToCreated(eventStoreConnection));
        }

        [Theory, AutoMoqData]
        public void Close_ChangeConnectionStateClosed(
            [Frozen] Mock<IEventStoreConnectionState> stateMock,
            EventStoreConnection eventStoreConnection,
            string streamName)
        {
            eventStoreConnection.Close();

            stateMock.Verify(self => self.ChangeToClosing());
            stateMock.Verify(self => self.ChangeToClosed());
        }

        [Theory, AutoMoqData]
        public async Task CreateStreamConsumerAsync_WhenConsumerDoesNotExist_Throws(
            [Frozen] Mock<IEventStreamConsumersRegistry> consumerRegistryMock,
            EventStoreConnection eventStoreConnection,
            string streamName,
            EventStreamConsumerId consumerId)
        {
            consumerRegistryMock
                .Setup(self => self.IsResistedAsync(consumerId))
                .Returns(TaskDone.False);

            await Assert.ThrowsAsync<UnknownEventStreamConsumerException>(() =>
                eventStoreConnection.CreateStreamConsumerAsync(streamName, consumerId));
        }

        [Theory, AutoMoqData]
        public async Task CreateStreamConsumerAsync_WhenConsumerExists_CreatesIt(
            [Frozen] Mock<IEventStreamConsumersRegistry> consumerRegistryMock,
            EventStoreConnection eventStoreConnection,
            string streamName,
            EventStreamConsumerId consumerId)
        {
            consumerRegistryMock
                .Setup(self => self.IsResistedAsync(consumerId))
                .Returns(TaskDone.True);

            var consumer = await eventStoreConnection.CreateStreamConsumerAsync(streamName, consumerId);

            Assert.NotNull(consumer);
            Assert.IsType<EventStreamConsumer>(consumer);
        }
    }
}
