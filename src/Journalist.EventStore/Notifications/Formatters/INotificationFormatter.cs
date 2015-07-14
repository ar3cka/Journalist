using System.IO;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications.Formatters
{
    public interface INotificationFormatter
    {
        Stream ToBytes(EventStreamUpdated notification);

        object FromBytes(Stream notificationBytes);
    }
}
