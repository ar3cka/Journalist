using System;
using System.IO;
using System.Runtime.Serialization;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications.Formatters
{
    public class NotificationFormatter : INotificationFormatter
    {
        public Stream ToBytes(INotification notification)
        {
            Require.NotNull(notification, "notification");

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                notification.SaveTo(writer);
                writer.Flush();

                return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public INotification FromBytes(Stream notificationBytes)
        {
            Require.NotNull(notificationBytes, "notificationBytes");

            using (var streamReader = new StreamReader(notificationBytes))
            {
                var line = streamReader.ReadLine();
                if (line == null)
                {
                    throw new InvalidOperationException("Invalid notification property format.");
                }

                string key;
                string value;
                NotificationPropertyParser.Parse(line, out key, out value);
                if (key != NotificationPropertyKeys.Common.NOTIFICATION_TYPE || value.IsNullOrEmpty())
                {
                    throw new InvalidOperationException("Invalid notification property format. Missing notification type value.");
                }

                var notificationType = Type.GetType(value, true);
                var notification = (INotification)FormatterServices.GetUninitializedObject(notificationType);

                notification.RestoreFrom(streamReader);

                return notification;
            }
        }
    }
}
