using System.IO;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications
{
    public interface INotification
    {
        bool IsAddressedTo(EventStreamReaderId consumerId);

        INotification SendTo(EventStreamReaderId consumerId);

        INotification RedeliverTo(EventStreamReaderId consumerId);

        void SaveTo(StreamWriter writer);

        void RestoreFrom(StreamReader reader);

        bool IsAddressed{ get; }

        NotificationId NotificationId { get; }

        string NotificationType { get; }

        int DeliveryCount { get; }
    }
}
