using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Processing;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class NotificationHandlerStub : INotificationHandler
    {
        private readonly object m_lock = new object();
        private readonly List<INotification> m_receivedNotifications = new List<INotification>();
        private readonly bool m_throwOnNotificationHandling;

        public NotificationHandlerStub(bool throwOnNotificationHandling)
        {
            m_throwOnNotificationHandling = throwOnNotificationHandling;
        }

        public List<INotification> ReceivedNotifications => m_receivedNotifications;

        public Task HandleNotificationAsync(INotification notification)
        {
            if (m_throwOnNotificationHandling)
            {
                throw new NotImplementedException();
            }

            lock (m_lock)
            {
                m_receivedNotifications.Add(notification);
            }

            return TaskDone.Done;
        }
    }
}
