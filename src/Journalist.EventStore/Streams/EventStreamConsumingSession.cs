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
        private DateTimeOffset m_leaseAcquisitionTime;
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
            if (m_acquiredLease == null)
            {
                EnsureBlobExists();

                try
                {
                    var acquiredLeaseId = await m_blob.AcquireLeaseAsync();
                    await ManageLeaseTimoutAsync(acquiredLeaseId);

                    m_acquiredLease = acquiredLeaseId;
                    m_leaseAcquisitionTime = DateTimeOffset.UtcNow;
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
            else
            {
                await ManageLeaseTimoutAsync(m_acquiredLease);
            }

            if (m_acquiredLease == null)
            {
                await BreakLeasIfExpiredAsync();
            }

            return m_acquiredLease != null;
        }

        private async Task ManageLeaseTimoutAsync(string leaseId)
        {
            if (IsExpired())
            {
                var sessionsExpiresOn = DateTimeOffset.UtcNow.Add(m_leaseTimeout);
                m_sessionsExpiresOn = sessionsExpiresOn.AddMinutes(-1);
                m_blob.Metadata[Constants.MetadataProperties.SESSION_EXPIRES_ON] = sessionsExpiresOn.ToString("O");
                await m_blob.SaveMetadataAsync(leaseId);
            }
        }

        public async Task FreeAsync()
        {
            if (m_acquiredLease != null)
            {
                try
                {
                    await m_blob.ReleaseLeaseAsync(m_acquiredLease);
                }
                catch (Exception exception)
                {
                    s_logger.Error(
                        exception,
                        "Acquired at {AcquisitionTime} session's ({StreamName}, {ConsumerId}) lease releasing failed.",
                        m_leaseAcquisitionTime,
                        m_streamName,
                        m_consumerId);
                }
                finally
                {
                    m_acquiredLease = null;
                    m_leaseAcquisitionTime = DateTimeOffset.MinValue;
                    m_sessionsExpiresOn = DateTimeOffset.MinValue;
                }
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

                    s_logger.Warning("Acquired session's ({StreamName}, {ConsumerId}) lease has been broken.", m_streamName, m_acquiredLease);
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

        private bool IsExpired() => m_sessionsExpiresOn <= DateTimeOffset.UtcNow;

        public string StreamName => m_streamName;

        public EventStreamReaderId ConsumerId => m_consumerId;
    }
}
