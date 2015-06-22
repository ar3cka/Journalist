using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.StreamCursor;

namespace Journalist.EventStore.Journal
{
    public interface IEventJournal
    {
        Task<EventStreamPosition> AppendEventsAsync(string streamName, EventStreamPosition position, IReadOnlyCollection<JournaledEvent> events);

        Task<EventStreamCursor> OpenEventStreamCursorAsync(string streamName, int sliceSize);

        Task<EventStreamCursor> OpenEventStreamCursorAsync(string streamName, StreamVersion fromVersion, int sliceSize);

        Task<EventStreamCursor> OpenEventStreamCursorAsync(string streamName, StreamVersion fromVersion, StreamVersion toVersion, int sliceSize);

        Task<EventStreamPosition> ReadEndOfStreamPositionAsync(string streamName);

        Task<StreamVersion> ReadStreamReaderPositionAsync(string streamName, string readerName);

        Task CommitStreamReaderPositionAsync(string streamName, string readerName, StreamVersion version);
    }
}
