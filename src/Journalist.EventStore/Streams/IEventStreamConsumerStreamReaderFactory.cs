using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConsumerStreamReaderFactory
    {
        Task<IEventStreamReader> CreateAsync();
    }
}
