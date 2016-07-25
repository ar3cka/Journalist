using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class StreamConsumingNotificationListenerStub : StreamConsumingNotificationListener
    {
        protected override Task<EventProcessingResult> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer, StreamVersion notificationStreamVersion)
        {
	        return ProcessingCompleted
		        ? Task.FromResult(new EventProcessingResult(true, true))
		        : Task.FromResult(new EventProcessingResult(false, false));
        }

        public bool ProcessingCompleted { get; set; }
    }
}
