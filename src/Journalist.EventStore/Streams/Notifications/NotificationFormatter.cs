using System.IO;
using Journalist.IO;

namespace Journalist.EventStore.Streams.Notifications
{
    public class NotificationFormatter : INotificationFormatter
    {
        public Stream ToBytes(EventStreamUpdated notification)
        {
            return EmptyMemoryStream.Get();
        }
    }
}