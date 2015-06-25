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

        public async Task<IEventStreamConsumer> CreateStreamConsumer(string streamName, string consumerName)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(consumerName, "consumerName");

            var readerVersion = await m_journal.ReadStreamReaderPositionAsync(
                streamName: streamName,
                readerName: consumerName);

            var reader = await CreateStreamReaderAsync(
                streamName: streamName,
                streamVersion: readerVersion.Increment());

            var session = m_sessionFactory.CreateSession(
                consumerName: consumerName,
                streamName: streamName);

            return new EventStreamConsumer(
                consumerName,
                reader,
                session,
                readerVersion,
                currentVersion => m_journal.CommitStreamReaderPositionAsync(
                    streamName: streamName,
                    readerName: consumerName,
                    version: currentVersion));
        }
    }
}
