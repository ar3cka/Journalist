using System;
using System.Threading.Tasks;
using Journalist.EventStore.Utils.Cloud.Azure.Storage.Blobs;
using Journalist.WindowsAzure.Storage;
using Journalist.WindowsAzure.Storage.Blobs;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Utils.Cloud.Azure.Storage.Blobs
{
    public class LeaseTests
    {
        public LeaseTests()
        {
            var blobContainer = new StorageFactory().CreateBlobContainer(
                "UseDevelopmentStorage=true",
                "leasetests");

            Blob = blobContainer.CreateBlockBlob(Guid.NewGuid().ToString("N"));
        }

        [Fact]
        public async Task IsAcquired_WhenLeaseWasAcquired_ReturnsTrue()
        {
            var lease = await Lease.AcquireAsync(Blob, TimeSpan.FromMinutes(1));

            Assert.True(Lease.IsAcquired(lease));
        }

        [Fact]
        public async Task IsAcquired_WhenLeaseWasNotAcquired_ReturnsTrue()
        {
            // arrange
            //
            await Lease.AcquireAsync(Blob, TimeSpan.FromMinutes(1));

            // act
            //
            var lease = await Lease.AcquireAsync(Blob, TimeSpan.FromMinutes(1));

            // assert
            //
            Assert.False(Lease.IsAcquired(lease));
        }

        [Fact]
        public async Task IsAcquired_WhenLeaseWasReleasedNotAcquired_ReturnsTrue()
        {
            // arrange
            //
            var lease = await Lease.AcquireAsync(Blob, TimeSpan.FromMinutes(1));
            lease = await Lease.ReleaseAsync(Blob, lease);

            // act
            //
            lease = await Lease.AcquireAsync(Blob, TimeSpan.FromMinutes(1));

            // assert
            //
            Assert.True(Lease.IsAcquired(lease));
        }

        public ICloudBlockBlob Blob { get; set; }
    }
}
