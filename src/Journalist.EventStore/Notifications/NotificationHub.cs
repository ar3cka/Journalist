using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Timeouts;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications
{
    public class NotificationHub : INotificationHub
    {
        private readonly List<NotificationListenerSubscription> m_subscriptions = new List<NotificationListenerSubscription>();
        private readonly INotificationsChannel m_channel;
        private readonly INotificationFormatter m_formatter;
        private readonly IPollingTimeout m_timeout;

        private CancellationTokenSource m_token;
        private Task m_processingTask;

        public NotificationHub(
            INotificationsChannel channel,
            INotificationFormatter formatter,
            IPollingTimeout timeout)
        {
            Require.NotNull(channel, "channel");
            Require.NotNull(formatter, "formatter");
            Require.NotNull(timeout, "timeout");

            m_channel = channel;
            m_formatter = formatter;
            m_timeout = timeout;
        }

        public Task NotifyAsync(EventStreamUpdated notification)
        {
            Require.NotNull(notification, "notification");

            return m_channel.SendAsync(m_formatter.ToBytes(notification));
        }

        public void Subscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            m_subscriptions.Add(new NotificationListenerSubscription(listener));
        }

        public void StartNotificationProcessing()
        {
            foreach (var subscriptions in m_subscriptions)
            {
                subscriptions.Start();
            }

            m_token = new CancellationTokenSource();
            m_processingTask = ProcessNotificationFromChannel(m_token.Token);
        }

        public void StopNotificationProcessing()
        {
            if (m_token != null)
            {
                m_token.Cancel();
                m_processingTask.Wait();
            }

            foreach (var subscriptions in m_subscriptions)
            {
                subscriptions.Stop();
            }
        }

        private async Task ProcessNotificationFromChannel(CancellationToken token)
        {
            // switch to the background task
            await Task.Yield();

            while (!token.IsCancellationRequested)
            {
                var notifications = await m_channel.ReceiveNotificationsAsync();
                if (notifications.IsEmpty())
                {
                    await m_timeout.WaitAsync(token);

                    m_timeout.Increase();
                }
                else
                {
                    m_timeout.Reset();

                    foreach (var notificationBytes in notifications)
                    {
                        var notification = m_formatter.FromBytes(notificationBytes);

                        foreach (var subscription in m_subscriptions)
                        {
                            await subscription.HandleNotificationAsync(notification);
                        }
                    }
                }
            }
        }
    }
}
