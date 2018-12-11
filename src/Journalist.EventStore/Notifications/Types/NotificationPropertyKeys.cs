namespace Journalist.EventStore.Notifications.Types
{
    internal static class NotificationPropertyKeys
    {
        public static class Common
        {
            public const string NOTIFICATION_ID = "NotificationId";
            public const string NOTIFICATION_TYPE = "NotificationType";
            public const string RECIPIENT = "Recipient";
            public const string STREAM = "Stream";
            public const string DELIVERY_COUNT = "DeliveryCount";
        }

        public static class EventStreamUpdated
        {
            public const string FROM_VERSION = "FromFromVersion";
            public const string TO_VERSION = "ToVersion";
        }
    }
}
