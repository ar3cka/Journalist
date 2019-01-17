using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Persistence;
using Journalist.EventStore.Notifications.Processing;
using Journalist.WindowsAzure.Storage.Queues;
using Serilog;

namespace Journalist.EventStore.Notifications.Channels
{
    public class NotificationsChannel : INotificationsChannel
    {
        private static readonly ILogger s_logger = Log.ForContext<NotificationsChannel>();
        private static readonly TimeSpan s_lockTimeout = TimeSpan.FromMinutes(Constants.Settings.NOTIFICATION_MESSAGE_LOCK_TIMEOUT_MINUTES);

        private readonly ICloudQueue[] m_queues;
        private readonly INotificationFormatter m_formatter;
        private readonly INotificationDeliveryTimeoutCalculator m_notificationDeliveryTimeoutCalculator;
        private readonly IFailedNotifications m_failedNotifications;

        private readonly int m_queueCount;
        private int m_incomingQueueIndex;
        private int m_outgoingQueueIndex;

        public NotificationsChannel(
            ICloudQueue[] queues,
            INotificationFormatter formatter,
            INotificationDeliveryTimeoutCalculator notificationDeliveryTimeoutCalculator,
            IFailedNotifications failedNotifications)
        {
            Require.NotNull(queues, nameof(queues));
            Require.NotNull(formatter, nameof(formatter));
            Require.NotNull(notificationDeliveryTimeoutCalculator, nameof(notificationDeliveryTimeoutCalculator));
            Require.NotNull(failedNotifications, nameof(failedNotifications));

            m_queues = queues;
            m_formatter = formatter;
            m_notificationDeliveryTimeoutCalculator = notificationDeliveryTimeoutCalculator;
            m_queueCount = queues.Length;
            m_failedNotifications = failedNotifications;

            var random = new Random();
            m_incomingQueueIndex = random.Next(0, m_queueCount);
            m_outgoingQueueIndex = random.Next(0, m_queueCount);
        }

        public Task SendAsync(INotification notification)
        {
            Require.NotNull(notification, nameof(notification));

            return SendInternalAsync(notification, null);
        }

        public Task SendAsync(INotification notification, TimeSpan visibilityTimeout)
        {
            Require.NotNull(notification, nameof(notification));

            return SendInternalAsync(notification, visibilityTimeout);
        }

        public async Task<IReceivedNotification[]> ReceiveNotificationsAsync()
        {
            var readedQueue = 0;
            do
            {
                var queue = ChooseIncomingQueue();
                var result = await ReadNotificationsFromQueueAsync(queue);

                if (result.Any())
                {
                    return result.ToArray();
                }

                readedQueue++;
            }
            while (readedQueue < m_queueCount);

            return EmptyArray.Get<IReceivedNotification>();
        }

        public async Task SendToFailedNotificationsAsync(INotification notification)
        {
            Require.NotNull(notification, nameof(notification));

            await m_failedNotifications.AddAsync(notification.CreateFailedNotification());
        }

        private ICloudQueue ChooseIncomingQueue()
        {
            int originalIndex;
            int chosenIndex;
            do
            {
                originalIndex = m_incomingQueueIndex;
                chosenIndex = originalIndex + 1;

                if (chosenIndex == m_queueCount)
                {
                    chosenIndex = 0;
                }
            }
            while (Interlocked.CompareExchange(ref m_incomingQueueIndex, chosenIndex, originalIndex) != originalIndex);

            return m_queues[chosenIndex];
        }

        private ICloudQueue ChooseOutgoingQueue()
        {
            int originalIndex;
            int chosenIndex;
            do
            {
                originalIndex = m_outgoingQueueIndex;
                chosenIndex = originalIndex + 1;

                if (chosenIndex == m_queueCount)
                {
                    chosenIndex = 0;
                }
            }
            while (Interlocked.CompareExchange(ref m_outgoingQueueIndex, chosenIndex, originalIndex) != originalIndex);

            return m_queues[chosenIndex];
        }

        private async Task<List<IReceivedNotification>> ReadNotificationsFromQueueAsync(ICloudQueue queue)
        {
            var messages = await queue.GetMessagesAsync(s_lockTimeout);
            var result = new List<IReceivedNotification>();
            foreach (var message in messages)
            {
                try
                {
                    using (var memory = new MemoryStream(message.Content))
                    {
                        result.Add(new ReceivedNotification(
                            queue,
                            message,
                            m_formatter.FromBytes(memory),
                            m_notificationDeliveryTimeoutCalculator,
                            m_failedNotifications));
                    }
                }
                catch (Exception exception)
                {
                    s_logger.Error(
                        exception,
                        "Notification receiving failed. Content: {NotificationContent}.",
                        message.Content);
                }
            }
            return result;
        }

        private async Task SendInternalAsync(INotification notification, TimeSpan? visibilityTimeout)
        {
            try
            {
                var queue = ChooseOutgoingQueue();

                using (var notificationBytes = m_formatter.ToBytes(notification))
                {
                    if (visibilityTimeout.HasValue)
                    {
                        await queue.AddMessageAsync(notificationBytes.ToArray(), visibilityTimeout.Value);
                    }
                    else
                    {
                        await queue.AddMessageAsync(notificationBytes.ToArray());
                    }
                }
            }
            catch (Exception exception)
            {
                s_logger.Error(
                    exception,
                    "Notification sending failed. Notification: {@Notification}.",
                    notification);

                throw;
            }
        }
    }
}
