using System.IO;
using Journalist.EventStore.Notifications.Streams;

namespace Journalist.EventStore.Notifications.Formatters
{
    public interface INotificationFormatter
    {
        Stream ToBytes(EventStreamUpdated notification);

        object FromBytes(Stream notificationBytes);
    }
}
