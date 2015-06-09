using System;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Journalist.WindowsAzure.Storage.Blobs
{
    public class CloudBlobContainerAdapter : ICloudBlobContainer
    {
        private readonly Lazy<CloudBlobContainer> m_container;

        public CloudBlobContainerAdapter(Func<CloudBlobContainer> containerFactory)
        {
            Require.NotNull(containerFactory, "containerFactory");

            m_container = new Lazy<CloudBlobContainer>(containerFactory, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ICloudBlockBlob CreateBlockBlob(string resourceName)
        {
            Require.NotEmpty(resourceName, "resourceName");

            var blob = Container.GetBlockBlobReference(resourceName);

            return new CloudBlockBlobAdapter(blob);
        }

        private CloudBlobContainer Container
        {
            get { return m_container.Value; }
        }
    }
}
