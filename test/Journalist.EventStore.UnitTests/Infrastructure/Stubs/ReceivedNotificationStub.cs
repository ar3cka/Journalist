using System.Threading.Tasks;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class ReceivedNotificationStub : IReceivedNotification
    {
        public ReceivedNotificationStub(INotification notification)
        {
            Notification = notification;
        }

        public bool IsRetried { get; private set; }

        public bool IsCompleted { get; private set; }

        public INotification Notification { get; }

        public Task RetryAsync()
        {
            IsRetried = true;

            return TaskDone.Done;
        }

        public Task CompleteAsync()
        {
            IsCompleted = true;

            return TaskDone.Done;
        }
    }
}
