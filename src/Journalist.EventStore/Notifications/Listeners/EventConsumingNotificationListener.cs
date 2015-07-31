using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Notifications.Listeners
{
    public abstract class EventConsumingNotificationListener : StreamConsumingNotificationListener
    {
        protected override async Task<bool> TryProcessEventFromConsumerAsync(IEventStreamConsumer consumer)
        {
            foreach (var journaledEvent in consumer.EnumerateEvents())
            {
                var failed = false;
                try
                {
                    await ProcessEventAsync(journaledEvent);
                }
                catch (Exception exception)
                {
                    ListenerLogger.Error(
                        exception,
                        "Processing event {EventId} of type {EventType} from stream {Stream} failed.",
                        journaledEvent.EventId,
                        journaledEvent.EventTypeName,
                        consumer.StreamName);

                    failed = true;
                }

                if (failed)
                {
                    await consumer.CommitProcessedStreamVersionAsync(skipCurrent: true);
                    return false;
                }
            }

            return true;
        }

        protected abstract Task ProcessEventAsync(JournaledEvent journaledEvent);
    }
}
