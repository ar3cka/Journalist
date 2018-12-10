using System.IO;
using Journalist.EventStore.Notifications.Listeners;

namespace Journalist.EventStore.Notifications
{
    public interface INotification
    {
        bool IsAddressedTo(INotificationListener listener);

        INotification SendTo(INotificationListener listener);

        INotification RedeliverTo(INotificationListener listener);

        void SaveTo(StreamWriter writer);

        void RestoreFrom(StreamReader reader);

        bool IsAddressed{ get; }

        NotificationId NotificationId { get; }

        string NotificationType { get; }

        int DeliveryCount { get; }

        IFailedNotification CreateFailedNotification();
    }
}
