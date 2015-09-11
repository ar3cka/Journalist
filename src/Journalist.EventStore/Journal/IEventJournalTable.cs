using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal
{
    public interface IEventJournalTable
    {
        Task<IDictionary<string, object>> ReadStreamHeadPropertiesAsync(string streamName);

        Task<IDictionary<string, object>> ReadStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId);

        Task InserStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version);

        Task UpdateStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version, string etag);

        Task<OperationResult> InsertEventsAsync(string streamName, EventStreamHeader header, IReadOnlyCollection<JournaledEvent> events);

        Task<FetchEventsResult> FetchStreamEvents(string stream, StreamVersion fromVersion, StreamVersion toVersion, int sliceSize);
    }
}
