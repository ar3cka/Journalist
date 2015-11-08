using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Utils.Polling;
using Journalist.Extensions;
using Serilog;

namespace Journalist.EventStore.Notifications
{
    public class NotificationHub : INotificationHub
    {
        private static readonly ILogger s_logger = Log.ForContext<NotificationHub>();

        private readonly Dictionary<Type, NotificationListenerSubscription> m_subscriptions = new Dictionary<Type, NotificationListenerSubscription>();
        private readonly IPollingJob m_pollingJob;
        private readonly INotificationsChannel m_channel;

        private int m_processingCount;
        private int m_maxProcessingCount;

        public NotificationHub(
            IPollingJob pollingJob,
            INotificationsChannel channel)
        {
            Require.NotNull(pollingJob, "pollingJob");
            Require.NotNull(channel, "channel");

            m_pollingJob = pollingJob;
            m_channel = channel;
        }

        public Task NotifyAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            return m_channel.SendAsync(notification);
        }

        public void Subscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            m_subscriptions.Add(listener.GetType(), new NotificationListenerSubscription(m_channel, listener));
        }

        public void Unsubscribe(INotificationListener listener)
        {
            Require.NotNull(listener, "listener");

            m_subscriptions.Remove(listener.GetType());
        }

        public void StartNotificationProcessing(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            if (m_subscriptions.Any())
            {
                m_maxProcessingCount = Constants.Settings.MAX_NOTIFICATION_PROCESSING_COUNT * m_subscriptions.Count;

                foreach (var subscriptions in m_subscriptions.Values)
                {
                    subscriptions.Start(connection);
                }

                m_pollingJob.Start(async token =>
                {
                    var notifications = await ReceiveNotificationsAsync();

                    if (notifications.IsEmpty())
                    {
                        s_logger.Debug("No notifications for processing.");

                        return false;
                    }

                    foreach (var notification in notifications)
                    {
                        ProcessNotification(notification);
                    }

                    return true;
                });
            }
        }

        public void StopNotificationProcessing()
        {
            if (m_subscriptions.Any())
            {
                // Call sequence is important. First we stop receiving
                // of a new notifications then wait for completion of
                // received notifications processing.
                //
                m_pollingJob.Stop();

                // Absense of new a notifications in the subscription
                // stop phase is essential guarantee.
                //
                foreach (var subscription in m_subscriptions.Values)
                {
                    subscription.Stop();
                }
            }
        }

        private bool RequestNotificationsRequired()
        {
            var observedProcessingCount = m_processingCount;
            if (observedProcessingCount >= m_maxProcessingCount)
            {
                s_logger.Debug(
                    "Number of notification processing ({ProcessingCount}) exceeded maximum value ({MaxProcessingCount}).",
                    observedProcessingCount,
                    m_maxProcessingCount);

                return false;
            }

            return true;
        }

        private async Task<IReceivedNotification[]> ReceiveNotificationsAsync()
        {
            var notifications = EmptyArray.Get<IReceivedNotification>();
            if (RequestNotificationsRequired())
            {
                notifications = await m_channel.ReceiveNotificationsAsync();

                s_logger.Debug(
                    "Receive {NotificationCount} notifications {NotificationIds}.",
                    notifications.Length,
                    notifications.Select(n => n.Notification.NotificationId));
            }

            return notifications;
        }

#pragma warning disable 4014
        private void ProcessNotification(IReceivedNotification notification)
        {
            var processingTasks = new List<Task<bool>>();
            foreach (var subscription in m_subscriptions.Values)
            {
                Interlocked.Increment(ref m_processingCount);

                processingTasks.Add(subscription
                    .HandleNotificationAsync(notification.Notification)
                    .ContinueWith(handlingTask =>
                    {
                        var hasError = false;
                        if (handlingTask.Exception != null)
                        {
                            s_logger.Fatal(
                                handlingTask.Exception.GetBaseException(),
                                "UNHANDLED EXCEPTION in NotificationListenerSubscription.");

                            hasError = true;
                        }

                        Interlocked.Decrement(ref m_processingCount);
                        return hasError;
                    }));
            }

            Task.WhenAll(processingTasks)
                .ContinueWith(async resultTask =>
                {
                    if (resultTask.Result.Any(hasError => hasError))
                    {
                        await notification.RetryAsync();
                    }
                    else
                    {
                        await notification.CompleteAsync();
                    }
                });
        }
#pragma warning restore 4014
    }
}
