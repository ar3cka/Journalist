using System;
using System.IO;
using Journalist.EventStore.Notifications.Streams;
using Journalist.IO;

namespace Journalist.EventStore.Notifications.Formatters
{
    public class NotificationFormatter : INotificationFormatter
    {
        public Stream ToBytes(EventStreamUpdated notification)
        {
            return EmptyMemoryStream.Get();
        }

        public object FromBytes(Stream notificationBytes)
        {
            throw new NotImplementedException();
        }
    }
}
