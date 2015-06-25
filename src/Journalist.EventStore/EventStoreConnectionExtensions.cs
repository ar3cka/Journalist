using System.Threading.Tasks;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore
{
    public static class EventStoreConnectionExtensions
    {
        public static Task<IEventStreamConsumer> CreateStreamConsumer(this IEventStoreConnection connection, string streamName)
        {
            Require.NotNull(connection, "connection");

            return connection.CreateStreamConsumer(streamName, Constants.DEFAULT_STREAM_READER_NAME);
        }
    }
}
