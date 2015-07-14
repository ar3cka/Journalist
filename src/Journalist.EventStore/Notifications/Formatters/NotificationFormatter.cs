using System;
using System.IO;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications.Formatters
{
    public class NotificationFormatter : INotificationFormatter
    {
        public Stream ToBytes(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("NotificationType: {0}", typeof(EventStreamUpdated).Name);
                writer.WriteLine("Stream: {0}", notification.StreamName);
                writer.WriteLine("FromVersion: {0}", (int)notification.FromVersion);
                writer.WriteLine("ToVersion: {0}", (int)notification.ToVersion);

                writer.Flush();

                return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public object FromBytes(Stream notificationBytes)
        {
            throw new NotImplementedException();
        }
    }
}
