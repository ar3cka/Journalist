using System.Threading.Tasks;

namespace Journalist.EventStore.Journal
{
    public interface IEventStreamCursor
    {
        Task FetchSlice();

        IEventStreamSlice Slice { get; }

        EventStreamPosition StreamPosition { get; }

        StreamVersion StreamVersion { get; }

        StreamVersion CursorStreamVersion { get; }

        bool EndOfStream { get; }
    }
}
