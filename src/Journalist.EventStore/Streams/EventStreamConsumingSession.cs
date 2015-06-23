using System;
using System.IO;
using System.Threading.Tasks;
using Journalist.Tasks;
using Journalist.WindowsAzure.Storage.Blobs;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumingSession : IEventStreamConsumingSession
    {
        private readonly string m_streamName;
        private readonly ICloudBlobContainer m_blobContainer;
        private ICloudBlockBlob m_blob;

        public EventStreamConsumingSession(string streamName, ICloudBlobContainer blobContainer)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(blobContainer, "blobContainer");

            m_streamName = streamName;
            m_blobContainer = blobContainer;
        }

        public async Task<bool> PromoteToLeaderAsync(string consumerId)
        {
            Require.NotEmpty(consumerId, "consumerId");

            EnsureBlobExists(consumerId);
            var leaseId = await m_blob.AcquireLeaseAsync(null);

            return leaseId != null;
        }

        public Task FreeAsync(string consumerId)
        {
            return TaskDone.Done;
        }

        private void EnsureBlobExists(string consumerId)
        {
            if (m_blob == null)
            {
                m_blob = m_blobContainer.CreateBlockBlob(consumerId + "/" + m_streamName);
            }
        }

        public string StreamName
        {
            get
            {
                return m_streamName;
            }
        }
    }
}
