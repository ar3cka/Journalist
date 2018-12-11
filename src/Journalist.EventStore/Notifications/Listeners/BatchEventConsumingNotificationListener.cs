using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications.Listeners
{
    public abstract class BatchEventConsumingNotificationListener : StreamConsumingNotificationListener
    {
        protected override async Task<EventProcessingResult> TryProcessEventFromConsumerAsync(
			IEventStreamConsumer consumer, 
			StreamVersion notificationStreamVersion)
        {
            try
            {
                await ProcessEventBatchAsync(consumer.EnumerateEvents().ToArray());

                return new EventProcessingResult(true, true);
            }
            catch (Exception exception)
            {
                ListenerLogger.Error(
                    exception,
                    "Processing event batch from stream {Stream} failed.",
                    consumer.StreamName);

                return new EventProcessingResult(false, false);
            }
        }

        protected abstract Task ProcessEventBatchAsync(JournaledEvent[] journaledEvent);
    }
}
