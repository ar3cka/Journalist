using System;
using System.IO;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.WindowsAzure.Storage.Blobs;
using Xunit;

namespace Journalist.WindowsAzure.Storage.IntegrationTests.Blobs
{
    public class CloudBlobContainerTests
    {
        public CloudBlobContainerTests()
        {
            Factory = new StorageFactory();
            Container = Factory.CreateBlobContainer("UseDevelopmentStorage=true", "cloud-block-blob-tests");
        }

        [Fact]
        public async Task CreateBlockBlob_WithDirectoryPathTest()
        {
            var blob = Container.CreateBlockBlob(
                Guid.NewGuid().ToString("D") +
                "/" +
                Guid.NewGuid().ToString("D"));

            await blob.UploadAsync(new MemoryStream(EmptyArray.Get<byte>()));

            Assert.True(await blob.IsExistsAsync());
        }

        public ICloudBlobContainer Container { get; set; }

        public StorageFactory Factory { get; set; }
    }
}
