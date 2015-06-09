using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Tasks;

namespace Journalist.EventStore.Streams
{
    public class EventStream : IEventStream
    {
        private readonly IEventJournal m_journal;
        private readonly IEventSerializer m_serializer;

        public EventStream(IEventJournal journal, IEventSerializer serializer)
        {
            Require.NotNull(journal, "journal");
            Require.NotNull(serializer, "serializer");

            m_journal = journal;
            m_serializer = serializer;
        }

        public async Task<IEventStreamReader> OpenReaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var reader = new EventStreamReader(
                streamName,
                await m_journal.OpenEventStreamAsync(streamName),
                m_serializer);

            return reader;
        }

        public Task<IEventStreamWriter> OpenWriterAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            throw new System.NotImplementedException();
        }
    }
}
