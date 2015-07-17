using System.Collections.Generic;

namespace Journalist.EventStore.Notifications
{
    public class NotificationHubController : INotificationHubController
    {
        private readonly List<NotificationHub> m_ownedHubs;

        public NotificationHubController(List<NotificationHub> ownedHubs)
        {
            Require.NotNull(ownedHubs, "ownedHubs");

            m_ownedHubs = ownedHubs;
        }

        public void StartHub(INotificationHub notificationHub)
        {
            Require.NotNull(notificationHub, "notificationHub");

            var hub = m_ownedHubs.Find(_ => ReferenceEquals(_, notificationHub));
            hub.StartNotificationProcessing();
        }

        public void StopHub(INotificationHub notificationHub)
        {
            Require.NotNull(notificationHub, "notificationHub");

            var hub = m_ownedHubs.Find(_ => ReferenceEquals(_, notificationHub));
            hub.StopNotificationProcessing();
        }
    }
}
