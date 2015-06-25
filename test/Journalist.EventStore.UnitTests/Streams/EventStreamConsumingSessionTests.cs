using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.Tasks;
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
            await session.PromoteToLeaderAsync();

            containerMock.Verify(self => self.CreateBlockBlob(session.ConsumerName + "/" + session.StreamName));
        }

        [Theory]
        [CloudBlockBlobContainerData(leaseLocked: true)]
        public async Task PromoteToLeader_WhenBlobLeaseLocked_ReturnsFalse(
            string consumerId,
            EventStreamConsumingSession session)
        {
            Assert.False(await session.PromoteToLeaderAsync());
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_WhenBlobLeaseIsNotLocked_ReturnsTrue(
            string consumerId,
            EventStreamConsumingSession session)
        {
            Assert.True(await session.PromoteToLeaderAsync());
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_AcquiresBlobLease(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            string consumerId,
            EventStreamConsumingSession session)
        {
            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.AcquireLeaseAsync(null));
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_WhenBlobLeaseHaveBeenAcquiredForFirstTime_UpdateMetadata(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            [Frozen] IDictionary<string, string> metadata,
            string leaseId,
            string consumerId,
            EventStreamConsumingSession session)
        {
            blobMock
                .Setup(self => self.AcquireLeaseAsync(null))
                .Returns(leaseId.YieldTask());

            await session.PromoteToLeaderAsync();

            Assert.True(metadata.ContainsKey("SessionExpiresOn"));
            blobMock.Verify(self => self.SaveMetadataAsync(leaseId));
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_WhenBlobLeaseHaveBeenAlreadyAcquired_UpdatesMetadataOnce(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            [Frozen] IDictionary<string, string> metadata,
            string consumerId,
            EventStreamConsumingSession session)
        {
            await session.PromoteToLeaderAsync();
            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.SaveMetadataAsync(It.IsAny<string>()), Times.Once());
        }

        [Theory]
        [CloudBlockBlobContainerData(leaseLocked: true)]
        public async Task PromoteToLeader_WhenBlobLeaseHaveNotBeenAcquired_FetchesAttributes(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            string consumerId,
            EventStreamConsumingSession session)
        {
            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.FetchAttributesAsync(), Times.Once());
        }

        [Theory]
        [CloudBlockBlobContainerData(leaseLocked: true)]
        public async Task PromoteToLeader_WhenBlobLeaseHaveNotBeenAcquiredAndTimeoutExpired_BreaksLease(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            [Frozen] IDictionary<string, string> metadata,
            string consumerId,
            EventStreamConsumingSession session)
        {
            metadata["SessionExpiresOn"] = DateTimeOffset.UtcNow.AddMinutes(-10).ToString("O");

            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.BreakLeaseAsync(null), Times.Once());
        }

        [Theory]
        [CloudBlockBlobContainerData(leaseLocked: true)]
        public async Task PromoteToLeader_WhenBlobLeaseHaveNotBeenAcquiredAndTimeoutNotExpired_DoesNotBreakLease(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            [Frozen] IDictionary<string, string> metadata,
            string consumerId,
            EventStreamConsumingSession session)
        {
            metadata["SessionExpiresOn"] = DateTimeOffset.UtcNow.AddMinutes(10).ToString("O");

            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.BreakLeaseAsync(null), Times.Never());
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_WhenBlobLeaseHaveBeenAcquired_TakesLeaseOnce(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            string consumerId,
            EventStreamConsumingSession session)
        {
            await session.PromoteToLeaderAsync();
            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.AcquireLeaseAsync(null), Times.Once());
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task PromoteToLeader_WhenBlobLeaseWasFreed_TakesLeaseOnce(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            string consumerId,
            EventStreamConsumingSession session)
        {
            await session.PromoteToLeaderAsync();
            await session.FreeAsync();
            await session.PromoteToLeaderAsync();

            blobMock.Verify(self => self.AcquireLeaseAsync(null), Times.Exactly(2));
        }

        [Theory]
        [CloudBlockBlobContainerData]
        public async Task FreeAsync_ReleasesAcquiredBlobLease(
            [Frozen] Mock<ICloudBlockBlob> blobMock,
            string consumerId,
            string leaseId,
            EventStreamConsumingSession session)
        {
            blobMock
                .Setup(self => self.AcquireLeaseAsync(null))
                .Returns(leaseId.YieldTask());

            await session.PromoteToLeaderAsync();

            await session.FreeAsync();

            blobMock.Verify(self => self.ReleaseLeaseAsync(leaseId));
        }
    }
}
