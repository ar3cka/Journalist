using System.Threading.Tasks;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore
{
    public interface IEventStoreConnection
    {
        Task<IEventStreamReader> CreateStreamReaderAsync(string streamName);

        Task<IEventStreamReader> CreateStreamReaderAsync(string streamName, StreamVersion streamVersion);

        Task<IEventStreamWriter> CreateStreamWriterAsync(string streamName);

        Task<IEventStreamProducer> CreateStreamProducerAsync(string streamName);

        Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, string consumerName);

        void Close();
    }
}
