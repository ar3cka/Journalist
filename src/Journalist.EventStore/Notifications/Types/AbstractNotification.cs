using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications.Types
{
    public abstract class AbstractNotification : INotification
    {
        private NotificationId m_notificationId;
        private string m_notificationType;
        private string m_recipient;
        private int m_deliveryCount;

        protected AbstractNotification()
        {
            m_notificationId = NotificationId.Create();
            m_notificationType = GetType().FullName;
            m_recipient = null;
        }

        public bool IsAddressedTo(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            Ensure.True(m_recipient != null, "Notification was not addressed.");

            return m_recipient.Equals(listener.GetType().FullName);
        }

        public INotification SendTo(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            if (IsAddressed && IsAddressedTo(listener))
            {
                return this;
            }

            return CopyAndUpdate(properties =>
            {
                properties[NotificationPropertyKeys.Common.NOTIFICATION_ID] = NotificationId.Create().ToString();
                properties[NotificationPropertyKeys.Common.RECIPIENT] = listener.GetType().FullName;
            });
        }

        public INotification RedeliverTo(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            return CopyAndUpdate(properties =>
            {
                properties[NotificationPropertyKeys.Common.NOTIFICATION_ID] = Guid.NewGuid().ToString("N");
                properties[NotificationPropertyKeys.Common.RECIPIENT] = listener.GetType().FullName;

                if (m_deliveryCount > 0)
                {
                    properties[NotificationPropertyKeys.Common.DELIVERY_COUNT] = (m_deliveryCount - 1).ToInvariantString();
                }
            });
        }

        public void SaveTo(StreamWriter writer)
        {
            Require.NotNull(writer, "writer");

            const string format = "{0}: {1}";

            var properties = new Dictionary<string, string>();
            SaveCommonProperties(properties);
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

            RestoreCommonProperties(properties, true);
            RestoreFromProperties(properties);
        }

        protected abstract void SavePropertiesTo(Dictionary<string, string> properties);

        protected abstract void RestoreFromProperties(Dictionary<string, string> properties);

        private INotification CopyAndUpdate(Action<Dictionary<string, string>> update)
        {
            var properties = new Dictionary<string, string>();
            SaveCommonProperties(properties);
            SavePropertiesTo(properties);

            update(properties);

            var notification = (AbstractNotification)FormatterServices.GetUninitializedObject(GetType());
            notification.RestoreCommonProperties(properties, false);
            notification.RestoreFromProperties(properties);

            return notification;
        }

        private void SaveCommonProperties(Dictionary<string, string> properties)
        {
            properties[NotificationPropertyKeys.Common.NOTIFICATION_TYPE] = NotificationType;
            properties[NotificationPropertyKeys.Common.NOTIFICATION_ID] = NotificationId.ToString();

            if (m_recipient != null)
            {
                properties[NotificationPropertyKeys.Common.RECIPIENT] = m_recipient;
            }

            if (m_deliveryCount != 0)
            {
                properties[NotificationPropertyKeys.Common.DELIVERY_COUNT] = m_deliveryCount.ToInvariantString();
            }
        }

        private void RestoreCommonProperties(Dictionary<string, string> properties, bool channelDelivery)
        {
            m_notificationId = NotificationId.Parse(properties[NotificationPropertyKeys.Common.NOTIFICATION_ID]);
            m_notificationType = GetType().FullName;

            if (properties.ContainsKey(NotificationPropertyKeys.Common.RECIPIENT))
            {
                m_recipient = properties[NotificationPropertyKeys.Common.RECIPIENT];
            }

            if (channelDelivery)
            {
                m_deliveryCount = 1;
                string deliveryCount;
                if (properties.TryGetValue(NotificationPropertyKeys.Common.DELIVERY_COUNT, out deliveryCount))
                {
                    var value = int.Parse(deliveryCount);
                    value++;
                    m_deliveryCount = value;
                }
            }
        }

        public bool IsAddressed
        {
            get { return m_recipient != null; }
        }

        public NotificationId NotificationId
        {
            get { return m_notificationId; }
        }

        public string NotificationType
        {
            get { return m_notificationType; }
        }

        public int DeliveryCount
        {
            get { return m_deliveryCount; }
        }
    }
}
