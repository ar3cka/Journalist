using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Blobs;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumingSession : IEventStreamConsumingSession
    {
        private readonly string m_streamName;
        private readonly ICloudBlobContainer m_blobContainer;
        private ICloudBlockBlob m_blob;
        private string m_acquiredLease;

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

            if (m_acquiredLease == null)
            {
                EnsureBlobExists(consumerId);
                m_acquiredLease = await m_blob.AcquireLeaseAsync(null);
            }

            return m_acquiredLease != null;
        }

        public async Task FreeAsync()
        {
            if (m_acquiredLease != null)
            {
                await m_blob.ReleaseLeaseAsync(m_acquiredLease);
            }
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
