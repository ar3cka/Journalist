using System;
using System.IO;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications.Types
{
    public interface INotification
    {
        bool IsAddressedTo(EventStreamConsumerId consumerId);

        INotification SendTo(EventStreamConsumerId consumerId);

        void SaveTo(StreamWriter writer);

        void RestoreFrom(StreamReader reader);

        bool IsAddressed{ get; }

        Guid NotificationId { get; }

        string NotificationType { get; }
    }
}
