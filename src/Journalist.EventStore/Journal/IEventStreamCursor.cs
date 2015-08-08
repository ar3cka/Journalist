using System.Threading.Tasks;

namespace Journalist.EventStore.Journal
{
    public interface IEventStreamCursor
    {
        Task FetchSlice();

        IEventStreamSlice Slice { get; }

        StreamVersion CurrentVersion { get; }

        bool EndOfStream { get; }
    }
}
