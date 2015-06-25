using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public static class EventStreamConsumerExtensions
    {
        public static Task CommitProcessedStreamVersionAsync(this IEventStreamConsumer consumer)
        {
            Require.NotNull(consumer, "consumer");

            return consumer.CommitProcessedStreamVersionAsync(false);
        }
    }
}
