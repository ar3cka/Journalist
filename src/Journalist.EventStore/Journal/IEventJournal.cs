using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Journal.StreamCursor;

namespace Journalist.EventStore.Journal
{
    public interface IEventJournal
    {
        Task<EventStreamPosition> AppendEventsAsync(string streamName, EventStreamPosition position, IReadOnlyCollection<JournaledEvent> events);

        Task<EventStreamCursor> OpenEventStreamAsync(string streamName);

        Task<EventStreamCursor> OpenEventStreamAsync(string streamName, int sliceSize);

        Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion);

        Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion, int sliceSize);

        Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion, StreamVersion toVersion);

        Task<EventStreamCursor> OpenEventStreamAsync(string streamName, StreamVersion fromVersion, StreamVersion toVersion, int sliceSize);

        Task<EventStreamPosition> ReadEndOfStreamPositionAsync(string streamName);
    }
}