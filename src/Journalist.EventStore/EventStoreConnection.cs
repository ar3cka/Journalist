using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;
using Journalist.EventStore.Utils;

namespace Journalist.EventStore
{
    public class EventStoreConnection : IEventStoreConnection
    {
        private readonly IEventJournal m_journal;
        private readonly IEventStreamConsumingSessionFactory m_sessionFactory;

        public EventStoreConnection(
            IEventJournal journal,
            IEventStreamConsumingSessionFactory sessionFactory)
        {
            Require.NotNull(journal, "journal");
            Require.NotNull(sessionFactory, "sessionFactory");

            m_journal = journal;
            m_sessionFactory = sessionFactory;
        }

        public async Task<IEventStreamReader> CreateStreamReaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var reader = new EventStreamReader(
                streamName,
                await m_journal.OpenEventStreamCursorAsync(streamName),
                version => m_journal.OpenEventStreamCursorAsync(streamName, version));

            return reader;
        }

        public async Task<IEventStreamReader> CreateStreamReaderAsync(string streamName, StreamVersion streamVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            var reader = new EventStreamReader(
                streamName,
                await m_journal.OpenEventStreamCursorAsync(streamName, streamVersion),
                version => m_journal.OpenEventStreamCursorAsync(streamName, version));

            return reader;
        }

        public async Task<IEventStreamWriter> CreateStreamWriterAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            var endOfStream = await m_journal.ReadEndOfStreamPositionAsync(streamName);

            return new EventStreamWriter(
                streamName,
                endOfStream,
                m_journal);
        }

        public async Task<IEventStreamProducer> CreateStreamProducer(string streamName)
        {
            return new EventStreamProducer(await CreateStreamWriterAsync(streamName), RetryPolicy.Default);
        }

        public async Task<IEventStreamConsumer> CreateStreamConsumer(string streamName)
        {
            var readerVersion = await m_journal.ReadStreamReaderPositionAsync(
                streamName,
                Constants.DEFAULT_STREAM_READER_NAME);

            var reader = await CreateStreamReaderAsync(streamName, readerVersion.Increment());

            return new EventStreamConsumer(
                Constants.DEFAULT_STREAM_READER_NAME,
                reader,
                m_sessionFactory.CreateSession(streamName),
                readerVersion,
                currentVersion => m_journal.CommitStreamReaderPositionAsync(
                    streamName,
                    Constants.DEFAULT_STREAM_READER_NAME,
                    currentVersion));
        }
    }
}
