using Serilog;

namespace Journalist.EventStore.Notifications
{
    public class PendingNotificationsChaser : IPendingNotificationsChaser
    {
        private static readonly ILogger s_logger = Log.ForContext<PendingNotificationsChaser>();

        private readonly IPendingNotifications m_pendingNotifications;
        private readonly INotificationHub m_notificationHub;

        public PendingNotificationsChaser(IPendingNotifications pendingNotifications, INotificationHub notificationHub)
        {
            Require.NotNull(pendingNotifications, "pendingNotifications");
            Require.NotNull(notificationHub, "notificationHub");

            m_pendingNotifications = pendingNotifications;
            m_notificationHub = notificationHub;
        }

        public void Start()
        {
            s_logger.Information("Starting pending notifications chaser...");

            // TODO

            s_logger.Information("Pending notifications chaser started.");
        }

        public void Stop()
        {
            s_logger.Information("Stoping pending notifications chaser...");

            // TODO

            s_logger.Information("Pending notifications chaser stoped.");
        }
    }
}
