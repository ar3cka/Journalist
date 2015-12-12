using System;
using System.Linq;
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
                        var notifications = await m_pendingNotifications.LoadAsync();
                        foreach (var notification in notifications)
                        {
                            await m_notificationHub.NotifyAsync(notification);
                            await m_pendingNotifications.DeleteAsync(notification.StreamName, notification.FromVersion);
                        }

                        result = notifications.Any();
                    }
                }
                catch (Exception exception)
                {
                    s_logger.Error(exception, "Notification processing failed.");
                }

                if (lease != null && Lease.IsAcquired(lease))
                {
                    await Lease.ReleaseAsync(m_chaserExclusiveAccessBlobLock, lease);
                }

                return result;
            });

            s_logger.Information("Pending notifications chaser started.");
        }

        public void Stop()
        {
            s_logger.Information("Stoping pending notifications chaser...");

            m_pollingJob.Stop();

            s_logger.Information("Pending notifications chaser stoped.");
        }
    }
}
