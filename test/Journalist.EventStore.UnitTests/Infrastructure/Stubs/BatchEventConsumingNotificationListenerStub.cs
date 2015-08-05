using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class BatchEventConsumingNotificationListenerStub : BatchEventConsumingNotificationListener
    {
        protected override Task ProcessEventBatchAsync(JournaledEvent[] journaledEvent)
        {
            if (Exception != null)
            {
                throw Exception;
            }

            return TaskDone.Done;
        }

        public Exception Exception { get; set; }
    }
}