using System;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.WindowsAzure.Storage.Blobs;
using Serilog;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumingSession : IEventStreamConsumingSession
    {
        private static readonly ILogger s_logger = Log.ForContext<EventStreamConsumingSession>();

        private readonly string m_streamName;
        private readonly EventStreamReaderId m_consumerId;
        private readonly TimeSpan m_leaseTimeout;
        private readonly ICloudBlobContainer m_blobContainer;

        private ICloudBlockBlob m_blob;
        private string m_acquiredLease;
        private DateTimeOffset m_sessionsExpiresOn;

        public EventStreamConsumingSession(
            string streamName,
            EventStreamReaderId consumerId,
            TimeSpan leaseTimeout,
            ICloudBlobContainer blobContainer)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(consumerId, "consumerId");
            Require.NotNull(blobContainer, "blobContainer");

            m_streamName = streamName;
            m_consumerId = consumerId;
            m_leaseTimeout = leaseTimeout;
            m_blobContainer = blobContainer;
        }

        public async Task<bool> PromoteToLeaderAsync()
        {
            string acquiredLease = null;
            if (m_acquiredLease == null)
            {
                EnsureBlobExists();
                try
                {
                    acquiredLease = await m_blob.AcquireLeaseAsync();
                }
                catch (LeaseAlreadyAcquiredException exception)
                {
                    s_logger.Debug(
                        exception,
                        "Promotion session ({StreamName}, {ConsumerId}) to leader failed.",
                        m_streamName,
                        m_consumerId);
                }
            }

            if (m_acquiredLease == null && acquiredLease != null)
            {
                m_acquiredLease = acquiredLease;

                await ManageLeaseTimoutAsync();
            }

            if (m_acquiredLease == null)
            {
                await BreakLeasIfExpiredAsync();
            }

            return m_acquiredLease != null;
        }

        private async Task ManageLeaseTimoutAsync()
        {
            if (m_sessionsExpiresOn <= DateTimeOffset.UtcNow)
            {
                var sessionsExpiresOn = DateTimeOffset.UtcNow.Add(m_leaseTimeout);
                m_sessionsExpiresOn = sessionsExpiresOn.AddMinutes(-1);
                m_blob.Metadata[Constants.MetadataProperties.SESSION_EXPIRES_ON] = sessionsExpiresOn.ToString("O");
                await m_blob.SaveMetadataAsync(m_acquiredLease);
            }
        }

        public async Task FreeAsync()
        {
            if (m_acquiredLease != null)
            {
                await m_blob.ReleaseLeaseAsync(m_acquiredLease);
                m_acquiredLease = null;
                m_sessionsExpiresOn = DateTimeOffset.MinValue;
            }
        }

        private async Task BreakLeasIfExpiredAsync()
        {
            await m_blob.FetchAttributesAsync();

            if (m_blob.Metadata.ContainsKey(Constants.MetadataProperties.SESSION_EXPIRES_ON))
            {
                var expiresOn = DateTimeOffset.Parse(m_blob.Metadata[Constants.MetadataProperties.SESSION_EXPIRES_ON]);
                if (expiresOn <= DateTimeOffset.UtcNow)
                {
                    await m_blob.BreakLeaseAsync();
                }
            }
        }

        private void EnsureBlobExists()
        {
            if (m_blob == null)
            {
                m_blob = m_blobContainer.CreateBlockBlob(m_consumerId + "/" + m_streamName);
            }
        }

        public string StreamName
        {
            get
            {
                return m_streamName;
            }
        }

        public EventStreamReaderId ConsumerId
        {
            get { return m_consumerId; }
        }
    }
}
