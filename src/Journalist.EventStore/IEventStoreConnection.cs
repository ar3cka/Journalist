using System.Threading.Tasks;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore
{
    public interface IEventStoreConnection
    {
        Task<IEventStreamReader> CreateStreamReaderAsync(string streamName);

        Task<IEventStreamReader> CreateStreamReaderAsync(string streamName, int streamVersion);

        Task<IEventStreamWriter> CreateStreamWriterAsync(string streamName);
    }
}
