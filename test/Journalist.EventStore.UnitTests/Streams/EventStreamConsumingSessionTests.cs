using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.WindowsAzure.Storage.Blobs;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamConsumingSessionTests
    {
        [Theory]
        [CloudBlockBlobContainerData(leaseLocked: true)]
        public async Task PromoteToLeader_CreatesBlobWithConsumerAndStreamSpecificPath(
            [Frozen] Mock<ICloudBlobContainer> containerMock,
            string consumerId,
            EventStreamConsumingSession session)
        {
            await session.PromoteToLeaderAsync(consumerId);

            containerMock.Verify(self => self.CreateBlockBlob(consumerId + "/" + session.StreamName));
        }

        [Theory]
        [CloudBlockBlobContainerData(leaseLocked: true)]
        public async Task PromoteToLeader_WhenBlobLeaseLocked_ReturnsFalse(
            string consumerId,
            EventStreamConsumingSession session)
        {
            Assert.False(await session.PromoteToLeaderAsync(consumerId));
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_WhenBlobLeaseIsNotLocked_ReturnsTrue(
            string consumerId,
            EventStreamConsumingSession session)
        {
            Assert.True(await session.PromoteToLeaderAsync(consumerId));
        }
    }
}
