using System.Threading.Tasks;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore
{
    public interface IEventStoreConnection
    {
        Task<IEventStreamReader> OpenReaderAsync(string streamName);

        Task<IEventStreamReader> OpenReaderAsync(string streamName, int streamVersion);

        Task<IEventStreamWriter> OpenWriterAsync(string streamName);
    }
}
