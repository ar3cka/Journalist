using System;
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

        public object FromBytes(Stream notificationBytes)
        {
            throw new NotImplementedException();
        }
    }
}
