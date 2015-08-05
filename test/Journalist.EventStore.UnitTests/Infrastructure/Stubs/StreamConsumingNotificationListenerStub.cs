using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Streams;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class StreamConsumingNotificationListenerStub : StreamConsumingNotificationListener
    {
        protected override Task<bool> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer)
        {
            return ProcessingCompleted ? TaskDone.True : TaskDone.False;
        }

        public bool ProcessingCompleted { get; set; }
    }
}
