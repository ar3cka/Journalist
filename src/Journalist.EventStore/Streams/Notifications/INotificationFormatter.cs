using System.IO;

namespace Journalist.EventStore.Streams.Notifications
{
    public interface INotificationFormatter
    {
        Stream ToBytes(EventStreamUpdated notification);

        object FromBytes(Stream notificationBytes);
    }
}
