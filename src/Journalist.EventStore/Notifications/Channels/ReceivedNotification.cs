using System;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Queues;

namespace Journalist.EventStore.Notifications.Channels
{
    public class ReceivedNotification : IReceivedNotification
    {
        private readonly ICloudQueue m_queue;
        private readonly ICloudQueueMessage m_notificationMessage;
        private readonly INotification m_notification;

        public ReceivedNotification(
            ICloudQueue queue,
            ICloudQueueMessage notificationMessage,
            INotification notification)
        {
            m_queue = queue;
            m_notificationMessage = notificationMessage;
            m_notification = notification;
        }

        public Task RetryAsync()
        {
            if (m_notificationMessage.DequeueCount > Constants.Settings.MAX_NOTIFICATION_PROCESSING_COUNT)
            {
                return CompleteAsync();
            }

            var timeout =
                Constants.Settings.NOTIFICATION_RETRY_DELIVERY_TIMEOUT_MULTIPLYER_SEC * m_notificationMessage.DequeueCount;

            return m_queue.UpdateMessageAsync(m_notificationMessage, TimeSpan.FromSeconds(timeout));
        }

        public Task CompleteAsync()
        {
            return m_queue.DeleteMessageAsync(m_notificationMessage);
        }

        public INotification Notification
        {
            get { return m_notification; }
        }
    }
}
