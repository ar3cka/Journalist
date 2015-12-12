using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Utils.Cloud.Azure.Storage.Blobs;
using Journalist.EventStore.Utils.Polling;
using Journalist.WindowsAzure.Storage.Blobs;
using Serilog;

namespace Journalist.EventStore.Notifications
{
    public class PendingNotificationsChaser : IPendingNotificationsChaser
    {
        private static readonly ILogger s_logger = Log.ForContext<PendingNotificationsChaser>();

        private readonly IPendingNotifications m_pendingNotifications;
        private readonly INotificationHub m_notificationHub;
        private readonly IPollingJob m_pollingJob;
        private readonly ICloudBlockBlob m_chaserExclusiveAccessBlobLock;

        public PendingNotificationsChaser(
            IPendingNotifications pendingNotifications,
            INotificationHub notificationHub,
            IPollingJob pollingJob,
            ICloudBlockBlob chaserExclusiveAccessBlobLock)
        {
            Require.NotNull(pendingNotifications, "pendingNotifications");
            Require.NotNull(notificationHub, "notificationHub");
            Require.NotNull(pollingJob, "pollingJob");
            Require.NotNull(chaserExclusiveAccessBlobLock, "chaserExclusiveAccessBlobLock");

            m_pendingNotifications = pendingNotifications;
            m_notificationHub = notificationHub;
            m_pollingJob = pollingJob;
            m_chaserExclusiveAccessBlobLock = chaserExclusiveAccessBlobLock;
        }

        public void Start()
        {
            s_logger.Information("Starting pending notifications chaser...");

            m_pollingJob.Start(async cancellationToken =>
            {
                var lease = Lease.NotAcquired;
                var result = false;

                try
                {
                    lease = await Lease.AcquireAsync(
                        m_chaserExclusiveAccessBlobLock,
                        TimeSpan.FromMinutes(Constants.Settings.PENDING_NOTIFICATIONS_CHASER_EXCLUSIVE_ACCESS_LOCK_TIMEOUT_IN_MINUTES));

                    if (Lease.IsAcquired(lease))
                    {
                        s_logger.Verbose("Chaser lock was successfully acquired. Start pending notification processing...");

                        var processNotificationCount = await ProcessPendingNotificationsAsync();

                        s_logger.Information("Chaser republished {NotificationCount} unpublished notifications.", processNotificationCount);

                        result = processNotificationCount != 0;
                    }
                    else
                    {
                        s_logger.Verbose("Chaser lock wasn't acquired.");
                    }
                }
                catch (Exception exception)
                {
                    s_logger.Error(exception, "Notification processing failed.");
                }

                await ReleaseLeaseAsync(lease);

                return result;
            });

            s_logger.Information("Pending notifications chaser started.");
        }

        private async Task<int> ProcessPendingNotificationsAsync()
        {
            var notifications = await m_pendingNotifications.LoadAsync();
            foreach (var notification in notifications)
            {
                await m_notificationHub.NotifyAsync(notification);
                await m_pendingNotifications.DeleteAsync(notification.StreamName, notification.FromVersion);
            }

            return notifications.Count;
        }

        private async Task ReleaseLeaseAsync(Lease lease)
        {
            if (Lease.IsAcquired(lease))
            {
                try
                {
                    await Lease.ReleaseAsync(m_chaserExclusiveAccessBlobLock, lease);

                    s_logger.Verbose("Chaser lock was successfully released.");
                }
                catch (Exception exception)
                {
                    s_logger.Error(exception, "Releasing chaser lock failed.");
                }
            }
        }

        public void Stop()
        {
            s_logger.Information("Stoping pending notifications chaser...");

            m_pollingJob.Stop();

            s_logger.Information("Pending notifications chaser stoped.");
        }
    }
}
