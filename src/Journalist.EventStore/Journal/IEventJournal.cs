using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Journal
{
    public interface IEventJournal
    {
        Task<EventStreamHeader> AppendEventsAsync(string streamName, EventStreamHeader header, IReadOnlyCollection<JournaledEvent> events);

        Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, int sliceSize);

        Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, StreamVersion fromVersion, int sliceSize);

        Task<IEventStreamCursor> OpenEventStreamCursorAsync(string streamName, EventStreamReaderId readerId, int sliceSize);

        Task<EventStreamHeader> ReadStreamHeaderAsync(string streamName);

        Task<StreamVersion> ReadStreamReaderPositionAsync(string streamName, EventStreamReaderId readerId);

	    Task<IEnumerable<StreamReaderDescription>> GetStreamReadersDescriptionsAsync(string streamName); 

        Task CommitStreamReaderPositionAsync(string streamName, EventStreamReaderId readerId, StreamVersion version);
    }
}
