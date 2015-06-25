using System;
using System.IO;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.WindowsAzure.Storage.Blobs;
using Xunit;

namespace Journalist.WindowsAzure.Storage.IntegrationTests.Blobs
{
    public class CloudBlockBlobTests
    {
        public CloudBlockBlobTests()
        {
            Factory = new StorageFactory();
            Container = Factory.CreateBlobContainer("UseDevelopmentStorage=true", "cloud-block-blob-tests");
            Blob = Container.CreateBlockBlob(Guid.NewGuid().ToString("D") + "/" );
        }

        [Fact]
        public async Task IsExistsAsync_WhenBlobNotCreated_ReturnsFalse()
        {
            Assert.False(await Blob.IsExistsAsync());
        }

        [Fact]
        public async Task IsExistsAsync_WhenBlobCreated_ReturnsTrue()
        {
            await Blob.UploadAsync(new MemoryStream(EmptyArray.Get<byte>()));

            Assert.True(await Blob.IsExistsAsync());
        }

        [Fact]
        public async Task IsExistsAsync_WhenBlobDeletedCreated_ReturnsFalse()
        {
            await Blob.UploadAsync(new MemoryStream(EmptyArray.Get<byte>()));
            await Blob.DeleteAsync();

            Assert.False(await Blob.IsExistsAsync());
        }

        [Fact]
        public async Task IsLeaseLocked_WhenBlobLeaseWasAcquired_ReturnsTrue()
        {
            await Blob.AcquireLeaseAsync(TimeSpan.FromSeconds(15));

            Assert.True(await Blob.IsLeaseLocked());
        }

        [Fact]
        public async Task IsLeaseLocked_WhenBlobLeaseWasNotAcquired_ReturnsFalse()
        {
            await Blob.UploadAsync(new MemoryStream(EmptyArray.Get<byte>()));

            Assert.False(await Blob.IsLeaseLocked());
        }

        [Fact]
        public async Task IsLeaseLocked_WhenBlobLeaseWasReleased_ReturnsFalse()
        {
            var leaseId = await Blob.AcquireLeaseAsync(TimeSpan.FromSeconds(15));
            await Blob.ReleaseLeaseAsync(leaseId);

            Assert.False(await Blob.IsLeaseLocked());
        }

        [Fact]
        public async Task BreakedLease_CanBeAcquired()
        {
            await Blob.AcquireLeaseAsync();

            await Blob.BreakLeaseAsync();

            Assert.NotNull(await Blob.AcquireLeaseAsync());
        }

        public ICloudBlockBlob Blob { get; set; }

        public ICloudBlobContainer Container { get; set; }

        public StorageFactory Factory { get; set; }
    }
}
