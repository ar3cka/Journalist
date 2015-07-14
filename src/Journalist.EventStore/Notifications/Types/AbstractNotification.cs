using System;
using System.Collections.Generic;
using System.IO;

namespace Journalist.EventStore.Notifications.Types
{
    public abstract class AbstractNotification : INotification
    {
        protected AbstractNotification()
        {
            NotificationId = Guid.NewGuid();
            NotificationType = GetType().FullName;
        }

        public void SaveTo(StreamWriter writer)
        {
            Require.NotNull(writer, "writer");

            const string format = "{0}: {1}";

            var properties = new Dictionary<string, string>();
            properties.Add(NotificationPropertyKeys.Common.NOTIFICATION_TYPE, NotificationType);
            properties.Add(NotificationPropertyKeys.Common.NOTIFICATION_ID, NotificationId.ToString("D"));

            SavePropertiesTo(properties);

            foreach (var property in properties)
            {
                writer.WriteLine(format, property.Key, property.Value);
            }
        }

        public void RestoreFrom(StreamReader reader)
        {
            Require.NotNull(reader, "reader");

            var properties = new Dictionary<string, string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string key;
                string value;
                NotificationPropertyParser.Parse(line, out key, out value);

                properties.Add(key, value);
            }

            NotificationId = Guid.Parse(properties[NotificationPropertyKeys.Common.NOTIFICATION_ID]);
            NotificationType = GetType().FullName;

            RestoreFromProperties(properties);
        }

        protected abstract void SavePropertiesTo(Dictionary<string, string> properties);

        protected abstract void RestoreFromProperties(Dictionary<string, string> properties);

        public Guid NotificationId
        {
            get;
            private set;
        }

        public string NotificationType
        {
            get;
            private set;
        }
    }
}
