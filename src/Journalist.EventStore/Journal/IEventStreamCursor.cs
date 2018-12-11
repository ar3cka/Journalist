using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal
{
    public interface IEventStreamCursor
    {
        Task FetchSlice();

        IEventStreamSlice Slice { get; }

        EventStreamHeader StreamHeader { get; }

        StreamVersion StreamVersion { get; }

        StreamVersion CursorStreamVersion { get; }

        bool EndOfStream { get; }
    }
}
