using System;
using System.IO;

namespace Journalist.EventStore.Notifications.Types
{
    public interface INotification
    {
        void SaveTo(StreamWriter writer);

        void RestoreFrom(StreamReader reader);

        Guid NotificationId { get; }

        string NotificationType { get; }
    }
}
