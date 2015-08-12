using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal
{
    public interface IEventJournal
    {
        Task<EventStreamPosition> AppendEventsAsync(string streamName, EventStreamPosition position, IReadOnlyCollection<JournaledEvent> events);

        Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, int sliceSize);

        Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, StreamVersion fromVersion, int sliceSize);

        Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, EventStreamReaderId readerId, int sliceSize);

        Task<EventStreamPosition> ReadEndOfStreamPositionAsync(string streamName);

        Task<StreamVersion> ReadStreamReaderPositionAsync(string streamName, EventStreamReaderId readerId);

        Task CommitStreamReaderPositionAsync(string streamName, EventStreamReaderId readerId, StreamVersion version);
    }
}
