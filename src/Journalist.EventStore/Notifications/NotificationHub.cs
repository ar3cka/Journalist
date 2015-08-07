using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Timeouts;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications
{
    public class NotificationHub : INotificationHub
    {
        private readonly Dictionary<EventStreamConsumerId, NotificationListenerSubscription> m_subscriptions = new Dictionary<EventStreamConsumerId, NotificationListenerSubscription>();
        private readonly Dictionary<INotificationListener, EventStreamConsumerId> m_listenerSubscriptions = new Dictionary<INotificationListener, EventStreamConsumerId>();
        private readonly INotificationsChannel m_channel;
        private readonly IEventStreamConsumersRegistry m_consumersRegistry;
        private readonly IPollingTimeout m_timeout;

        private CancellationTokenSource m_token;
        private Task m_processingTask;

        public NotificationHub(
            INotificationsChannel channel,
            IEventStreamConsumersRegistry consumersRegistry,
            IPollingTimeout timeout)
        {
            Require.NotNull(channel, "channel");
            Require.NotNull(consumersRegistry, "consumersRegistry");
            Require.NotNull(timeout, "timeout");

            m_channel = channel;
            m_consumersRegistry = consumersRegistry;
            m_timeout = timeout;
        }

        public Task NotifyAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            return m_channel.SendAsync(notification);
        }

        public void Subscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            var consumerId = RegisterEventListenerConsumer(listener);
            m_subscriptions.Add(consumerId, new NotificationListenerSubscription(consumerId, m_channel, listener));
            m_listenerSubscriptions.Add(listener, consumerId);
        }

        public void Unsubscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            var subscriptionId = m_listenerSubscriptions[listener];
            m_subscriptions.Remove(subscriptionId);
        }

        public void StartNotificationProcessing(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            if (m_subscriptions.Any())
            {
                foreach (var subscriptions in m_subscriptions.Values)
                {
                    subscriptions.Start(connection);
                }

                m_token = new CancellationTokenSource();
                m_processingTask = ProcessNotificationFromChannel(m_token.Token);
            }
        }

        public void StopNotificationProcessing()
        {
            if (m_subscriptions.Any())
            {
                if (m_token != null)
                {
                    m_token.Cancel();
                    m_processingTask.Wait();
                }

                foreach (var subscriptions in m_subscriptions.Values)
                {
                    subscriptions.Stop();
                }
            }
        }

        private EventStreamConsumerId RegisterEventListenerConsumer(INotificationListener listener)
        {
            return Task.Run(() =>
                m_consumersRegistry.RegisterAsync(listener.GetType().FullName)).Result;
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

                    foreach (var notification in notifications)
                    {
                        foreach (var subscription in m_subscriptions.Values)
                        {
                            await subscription.HandleNotificationAsync(notification);
                        }
                    }
                }
            }
        }
    }
}
