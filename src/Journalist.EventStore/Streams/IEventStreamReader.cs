using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public interface IEventStreamReader
    {
        Task ReadEventsAsync();

        IReadOnlyList<object> Events { get; }

        bool HasMoreEvents { get; }

        string StreamName { get; }
    }
}
