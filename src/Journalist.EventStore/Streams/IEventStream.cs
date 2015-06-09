using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStream
    {
        Task<IEventStreamReader> OpenReaderAsync(string streamName);

        Task<IEventStreamWriter> OpenWriterAsync(string streamName);
    }
}