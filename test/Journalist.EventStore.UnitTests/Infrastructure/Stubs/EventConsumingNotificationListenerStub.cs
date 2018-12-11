using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class EventConsumingNotificationListenerStub : EventConsumingNotificationListener
    {
        protected override Task ProcessEventAsync(JournaledEvent journaledEvent)
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
