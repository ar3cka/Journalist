using System.IO;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications
{
    public interface INotification
    {
        bool IsAddressedTo(EventStreamConsumerId consumerId);

        INotification SendTo(EventStreamConsumerId consumerId);

        INotification RedeliverTo(EventStreamConsumerId consumerId);

        void SaveTo(StreamWriter writer);

        void RestoreFrom(StreamReader reader);

        bool IsAddressed{ get; }

        NotificationId NotificationId { get; }

        string NotificationType { get; }

        int DeliveryCount { get; }
    }
}
