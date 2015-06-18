using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore
{
    public class EventStoreConnection : IEventStoreConnection
    {
        private readonly IEventJournal m_journal;

        public EventStoreConnection(IEventJournal journal)
        {
            Require.NotNull(journal, "journal");

            m_journal = journal;
        }

        public async Task<IEventStreamReader> OpenReaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var reader = new EventStreamReader(
                streamName,
                await m_journal.OpenEventStreamAsync(streamName));

            return reader;
        }

        public async Task<IEventStreamReader> OpenReaderAsync(string streamName, int streamVersion)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(streamVersion, "streamVersion");

            var reader = new EventStreamReader(
                streamName,
                await m_journal.OpenEventStreamAsync(streamName, StreamVersion.Create(streamVersion)));

            return reader;
        }

        public async Task<IEventStreamWriter> OpenWriterAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var endOfStream = await m_journal.ReadEndOfStreamPositionAsync(streamName);

            return new EventStreamWriter(
                streamName,
                endOfStream,
                m_journal);
        }
    }
}
