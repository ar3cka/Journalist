using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStream
    {
        Task<IEventStreamReader> OpenReaderAsync(string streamName);

        Task<IEventStreamReader> OpenReaderAsync(string streamName, int streamVersion);

        Task<IEventStreamWriter> OpenWriterAsync(string streamName);
    }
}
