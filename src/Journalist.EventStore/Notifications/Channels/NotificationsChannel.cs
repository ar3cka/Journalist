using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Types;
using Journalist.WindowsAzure.Storage.Queues;
using Serilog;

namespace Journalist.EventStore.Notifications.Channels
{
    public class NotificationsChannel : INotificationsChannel
    {
        private static readonly ILogger s_logger = Log.ForContext<NotificationsChannel>();

        private readonly ICloudQueue m_queue;
        private readonly INotificationFormatter m_formatter;

        public NotificationsChannel(ICloudQueue queue, INotificationFormatter formatter)
        {
            Require.NotNull(queue, "queue");
            Require.NotNull(formatter, "formatter");

            m_queue = queue;
            m_formatter = formatter;
        }

        public Task SendAsync(INotification notification)
        {
            Require.NotNull(notification, "notification");

            return SendInternalAsync(notification, null);
        }

        public Task SendAsync(INotification notification, TimeSpan visibilityTimeout)
        {
            Require.NotNull(notification, "notification");

            return SendInternalAsync(notification, visibilityTimeout);
        }

        public async Task<INotification[]> ReceiveNotificationsAsync()
        {
            var messages = await m_queue.GetMessagesAsync();

            var result = new List<INotification>();
            foreach (var message in messages)
            {
                try
                {
                    using (var memory = new MemoryStream(message.Content))
                    {
                        result.Add(m_formatter.FromBytes(memory));
                    }

                    await m_queue.DeleteMessageAsync(message);
                }
                catch (Exception exception)
                {
                    s_logger.Error(
                        exception,
                        "Notification receiving failed. Content: {NotificationContent}.",
                        message.Content);
                }
            }

            return result.ToArray();
        }

        private async Task SendInternalAsync(INotification notification, TimeSpan? visibilityTimeout)
        {
            try
            {
                using (var notificationBytes = m_formatter.ToBytes(notification))
                {
                    if (visibilityTimeout.HasValue)
                    {
                        await m_queue.AddMessageAsync(notificationBytes.ToArray(), visibilityTimeout.Value);
                    }
                    else
                    {
                        await m_queue.AddMessageAsync(notificationBytes.ToArray());
                    }
                }
            }
            catch (Exception exception)
            {
                s_logger.Error(
                    exception,
                    "Notification sending failed. Notification: {@Notification}.",
                    notification);
            }
        }
    }
}
