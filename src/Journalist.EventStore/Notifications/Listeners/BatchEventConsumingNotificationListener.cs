using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications.Listeners
{
    public abstract class BatchEventConsumingNotificationListener : StreamConsumingNotificationListener
    {
        protected async override Task<bool> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer)
        {
            try
            {
                await ProcessEventBatchAsync(consumer.EnumerateEvents().ToArray());

                return true;
            }
            catch (Exception exception)
            {
                ListenerLogger.Error(
                    exception,
                    "Processing event batch from stream {Stream} failed.",
                    consumer.StreamName);

                return false;
            }
        }

        protected abstract Task ProcessEventBatchAsync(JournaledEvent[] journaledEvent);
    }
}
