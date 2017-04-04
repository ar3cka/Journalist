using System.Collections.Generic;
using System.IO;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Notifications.Types
{
    public class EventStreamUpdated : AbstractNotification
    {
        private EventStreamUpdated()
        {
        }

        public EventStreamUpdated(string streamName, StreamVersion fromVersion, StreamVersion toVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            StreamName = streamName;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public static EventStreamUpdated RestoreFrom(MemoryStream memoryStream)
        {
            var notification = new EventStreamUpdated();
            using (var streamReader = new StreamReader(memoryStream))
            {
                notification.RestoreFrom(streamReader);
            }

            return notification;
        }

        protected override void SavePropertiesTo(Dictionary<string, string> properties)
        {
            properties.Add(NotificationPropertyKeys.Common.STREAM, StreamName);
            properties.Add(NotificationPropertyKeys.EventStreamUpdated.FROM_VERSION, ((int)FromVersion).ToString());
            properties.Add(NotificationPropertyKeys.EventStreamUpdated.TO_VERSION, ((int)ToVersion).ToString());
        }

        protected override void RestoreFromProperties(Dictionary<string, string> properties)
        {
            StreamName = properties[NotificationPropertyKeys.Common.STREAM];
            FromVersion = StreamVersion.Parse(properties[NotificationPropertyKeys.EventStreamUpdated.FROM_VERSION]);
            ToVersion = StreamVersion.Parse(properties[NotificationPropertyKeys.EventStreamUpdated.TO_VERSION]);
        }

        public string StreamName { get; private set; }

        public StreamVersion FromVersion { get; private set; }

        public StreamVersion ToVersion { get; private set; }
    }
}
