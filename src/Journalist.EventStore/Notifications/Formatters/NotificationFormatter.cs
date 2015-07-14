using System;
using System.Collections.Generic;
using System.IO;
using Journalist.Collections;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications.Formatters
{
    public class NotificationFormatter : INotificationFormatter
    {
        private static readonly string[] s_separators = ": ".YieldArray();

        private static class PropertyKeys
        {
            public static class Common
            {
                public const string NOTIFICATION_TYPE = "NotificationType";
                public const string STREAM = "Stream";
            }

            public static class EventStreamUpdated
            {
                public const string FROM_VERSION = "FromVersion";
                public const string TO_VERSION = "ToVersion";
            }
        }

        public Stream ToBytes(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                const string format = "{0}: {1}";
                writer.WriteLine(format, PropertyKeys.Common.NOTIFICATION_TYPE, typeof(EventStreamUpdated).FullName);
                writer.WriteLine(format, PropertyKeys.Common.STREAM, notification.StreamName);
                writer.WriteLine(format, PropertyKeys.EventStreamUpdated.FROM_VERSION, (int)notification.FromVersion);
                writer.WriteLine(format, PropertyKeys.EventStreamUpdated.TO_VERSION, (int)notification.ToVersion);
                writer.Flush();

                return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        public object FromBytes(Stream notificationBytes)
        {
            Require.NotNull(notificationBytes, "notificationBytes");

            var properties = ReadNotificationProperties(notificationBytes);
            var notificationType = Type.GetType(properties[PropertyKeys.Common.NOTIFICATION_TYPE], true);

            return CreateNotificationObjectFromProperties(notificationType, properties);
        }

        private static Dictionary<string, string> ReadNotificationProperties(Stream notificationBytes)
        {
            var properties = new Dictionary<string, string>();

            using (var streamReader = new StreamReader(notificationBytes))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    ReadProperty(properties, line);
                }
            }

            return properties;
        }

        private static object CreateNotificationObjectFromProperties(Type notificationType, Dictionary<string, string> properties)
        {
            switch (notificationType.Name)
            {
                case "EventStreamUpdated":
                    return new EventStreamUpdated(
                        properties[PropertyKeys.Common.STREAM],
                        StreamVersion.Parse(properties[PropertyKeys.EventStreamUpdated.FROM_VERSION]),
                        StreamVersion.Parse(properties[PropertyKeys.EventStreamUpdated.TO_VERSION]));
            }

            throw new InvalidOperationException("Unknown notification type \"{0}\".".FormatString(notificationType));
        }

        private static void ReadProperty(Dictionary<string, string> properties, string line)
        {
            var pair = line.Split(s_separators, StringSplitOptions.RemoveEmptyEntries);
            properties.Add(pair[0], pair[1]);
        }
    }
}
