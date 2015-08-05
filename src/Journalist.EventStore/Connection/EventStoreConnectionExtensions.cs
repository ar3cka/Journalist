using System.Threading.Tasks;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public static class EventStoreConnectionExtensions
    {
        public static Task<IEventStreamConsumer> CreateStreamConsumerAsync(this IEventStoreConnection connection, string streamName)
        {
            Require.NotNull(connection, "connection");

            return connection.CreateStreamConsumerAsync(config => config.ReadStream(streamName));
        }
    }
}
