using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
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
        public void Constructor_CreatesAndStartsNotificationHub(
            [Frozen] Mock<INotificationPipelineFactory> factoryMock,
            [Frozen] Mock<INotificationHubController> controllerMock,
            [Frozen] INotificationHub hub,
            EventStoreConnection eventStoreConnection)
        {
            factoryMock.Verify(self => self.CreateHub(), Times.Once());
            factoryMock.Verify(self => self.CreateHubController(), Times.Once());
            controllerMock.Verify(self => self.StartHub(hub));
        }

        [Theory, AutoMoqData]
        public void Close_StopsNotificationHub(
            [Frozen] Mock<INotificationHubController> controllerMock,
            [Frozen] INotificationHub hub,
            EventStoreConnection eventStoreConnection)
        {
            eventStoreConnection.Close();

            controllerMock.Verify(self => self.StopHub(hub));
        }
    }
}
