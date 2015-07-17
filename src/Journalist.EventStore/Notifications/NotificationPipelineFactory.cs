using System.Collections.Generic;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Timeouts;

namespace Journalist.EventStore.Notifications
{
    public class NotificationPipelineFactory : INotificationPipelineFactory
    {
        private readonly INotificationsChannel m_channel;
        private readonly IEnumerable<INotificationListener> m_listeners;
        private readonly List<NotificationHub> m_hubs;
        private readonly PollingTimeout m_pollingTimeout;
        private readonly NotificationFormatter m_notificationFormatter;

        public NotificationPipelineFactory(
            INotificationsChannel channel,
            IEnumerable<INotificationListener> listeners)
        {
            Require.NotNull(channel, "channel");
            Require.NotNull(listeners, "listeners");

            m_channel = channel;
            m_listeners = listeners;

            m_hubs = new List<NotificationHub>();
            m_pollingTimeout = new PollingTimeout();
            m_notificationFormatter = new NotificationFormatter();
        }

        public INotificationHub CreateHub()
        {
            var hub = new NotificationHub(
                m_channel,
                m_notificationFormatter,
                m_pollingTimeout);

            foreach (var notificationListener in m_listeners)
            {
                hub.Subscribe(notificationListener);
            }

            m_hubs.Add(hub);

            return hub;
        }

        public INotificationHubController CreateHubController()
        {
            return new NotificationHubController(m_hubs);
        }
    }
}
