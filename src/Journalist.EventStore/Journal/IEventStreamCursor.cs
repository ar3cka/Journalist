using System.Threading.Tasks;
using Journalist.EventStore.Journal.StreamCursor;

namespace Journalist.EventStore.Journal
{
    public interface IEventStreamCursor
    {
        Task FetchSlice();

        EventStreamSlice Slice { get; }

        StreamVersion CurrentVersion { get; }

        bool Fetching { get; }

        bool EndOfStream { get; }
    }
}