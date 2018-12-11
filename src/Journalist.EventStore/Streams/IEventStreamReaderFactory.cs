using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamReaderFactory
    {
        Task<IEventStreamReader> CreateAsync();
    }
}
