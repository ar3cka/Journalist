using System.Collections.Generic;
using Journalist.EventStore.Notifications.Listeners;

namespace Journalist.EventStore.Configuration
{
    public class NotificationProcessingConfiguration : INotificationProcessingConfiguration
    {
        private readonly IEventStoreConnectionConfiguration m_connection;
        private readonly List<INotificationListener> m_listeners;
        private bool m_enabled;

        public NotificationProcessingConfiguration(IEventStoreConnectionConfiguration connection, List<INotificationListener> listeners)
        {
            Require.NotNull(connection, "connection");
            Require.NotNull(listeners, "listeners");

            m_connection = connection;
            m_listeners = listeners;
        }

        public IEventStoreConnectionConfiguration EnableProcessing()
        {
            m_enabled = true;

            return m_connection;
        }

        public IEventStoreConnectionConfiguration Subscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            if (m_enabled)
            {
                m_listeners.Add(listener);
            }

            return m_connection;
        }
    }
}
