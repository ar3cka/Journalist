using System;
using Journalist.WindowsAzure.Storage.Internals;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Journalist.WindowsAzure.Storage.Blobs
{
    public class CloudBlobContainerAdapter : CloudEntityAdapter<CloudBlobContainer>, ICloudBlobContainer
    {
        public CloudBlobContainerAdapter(Func<CloudBlobContainer> containerFactory) : base(containerFactory)
        {
        }

        public ICloudBlockBlob CreateBlockBlob(string resourceName)
        {
            Require.NotEmpty(resourceName, "resourceName");

            var blob = CloudEntity.GetBlockBlobReference(resourceName);

            return new CloudBlockBlobAdapter(blob);
        }

    }
}
