using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public interface IEventStoreConnection
    {
        Task<IEventStreamReader> CreateStreamReaderAsync(string streamName);

        Task<IEventStreamReader> CreateStreamReaderAsync(string streamName, StreamVersion streamVersion);

        Task<IEventStreamWriter> CreateStreamWriterAsync(string streamName);

        Task<IEventStreamProducer> CreateStreamProducerAsync(string streamName);

        Task<IEventStreamConsumer> CreateStreamConsumerAsync(Action<IEventStreamConsumerConfiguration> configure);

        Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, string consumerName);

        void Close();
    }
}
