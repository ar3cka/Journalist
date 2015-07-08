using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public class NotificationHub : INotificationHub
    {
        private readonly List<Subscription> m_subscriptions = new List<Subscription>();

        private class Subscription : ISubscription
        {
            private readonly INotificationListener m_listener;
            private bool m_active;

            public Subscription(INotificationListener listener)
            {
                m_listener = listener;
            }

            public Task ProcessNotificationAsync(EventStreamUpdated notification)
            {
                if (m_active)
                {
                    return m_listener.OnEventStreamUpdatedAsync(notification);
                }

                return TaskDone.Done;
            }

            public void Start()
            {
                m_active = true;
            }

            public void Stop()
            {
                m_active = false;
            }
        }

        public async Task NotifyAsync(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            foreach (var subscription in m_subscriptions)
            {
                await subscription.ProcessNotificationAsync(notification);
            }
        }

        public ISubscription Subscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            var result = new Subscription(listener);
            m_subscriptions.Add(result);

            return result;
        }
    }
}
