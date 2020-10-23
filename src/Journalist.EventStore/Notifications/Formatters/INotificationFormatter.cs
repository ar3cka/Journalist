using System.IO;

namespace Journalist.EventStore.Notifications.Formatters
{
    public interface INotificationFormatter
    {
        MemoryStream ToBytes(INotification notification);

        INotification FromBytes(Stream notificationBytes);
    }
}
