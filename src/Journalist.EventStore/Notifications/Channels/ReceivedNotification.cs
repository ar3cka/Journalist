using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Persistence;
using Journalist.EventStore.Notifications.Processing;
using Journalist.WindowsAzure.Storage.Queues;

namespace Journalist.EventStore.Notifications.Channels
{
    public class ReceivedNotification : IReceivedNotification
    {
        private readonly ICloudQueue m_queue;
        private readonly ICloudQueueMessage m_notificationMessage;
        private readonly INotificationDeliveryTimeoutCalculator m_notificationDeliveryTimeoutCalculator;
        private readonly IFailedNotifications m_failedNotifications;

        public ReceivedNotification(
            ICloudQueue queue,
            ICloudQueueMessage notificationMessage,
            INotification notification,
            INotificationDeliveryTimeoutCalculator notificationDeliveryTimeoutCalculator,
            IFailedNotifications failedNotifications)
        {
            Require.NotNull(queue, nameof(queue));
            Require.NotNull(notificationMessage, nameof(notificationMessage));
            Require.NotNull(notification, nameof(notification));
            Require.NotNull(notificationDeliveryTimeoutCalculator, nameof(notificationDeliveryTimeoutCalculator));
            Require.NotNull(failedNotifications, nameof(failedNotifications));

            m_queue = queue;
            m_notificationMessage = notificationMessage;
            Notification = notification;
            m_notificationDeliveryTimeoutCalculator = notificationDeliveryTimeoutCalculator;
            m_failedNotifications = failedNotifications;
        }

        public Task RetryAsync()
        {
            if (m_notificationMessage.DequeueCount > Constants.Settings.MAX_NOTIFICATION_PROCESSING_COUNT)
            {
                return m_failedNotifications.AddAsync(Notification.CreateFailedNotification());
            }

            var timeout = m_notificationDeliveryTimeoutCalculator.CalculateDeliveryTimeout(Notification.DeliveryCount);

            return m_queue.UpdateMessageAsync(m_notificationMessage, timeout);
        }

        public Task CompleteAsync()
        {
            return m_queue.DeleteMessageAsync(m_notificationMessage);
        }

        public INotification Notification { get; }
    }
}
