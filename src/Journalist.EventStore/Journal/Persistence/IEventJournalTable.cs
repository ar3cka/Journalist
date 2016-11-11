using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.Journal.StreamCursor;

namespace Journalist.EventStore.Journal.Persistence
{
    public interface IEventJournalTable
    {
        AppendOperation CreateAppendOperation(string streamName, EventStreamHeader header);

        Task<IDictionary<string, object>> ReadStreamHeadPropertiesAsync(string streamName);

        Task<IDictionary<string, object>> ReadStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId);

        Task InserStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version);

        Task UpdateStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version, string etag);

        Task<FetchEventsResult> FetchStreamEvents(string stream, EventStreamHeader header, StreamVersion fromVersion, int sliceSize);
    }
}
