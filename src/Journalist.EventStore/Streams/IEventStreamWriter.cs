using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamWriter
    {
        int StreamPosition { get; }

        Task AppendEvents(IEnumerable<object> events);
    }
}
