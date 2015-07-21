using System.Threading.Tasks;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Streams;
using Journalist.EventStore.Utils;

namespace Journalist.EventStore.Connection
{
    public class EventStoreConnection : IEventStoreConnection
    {
        private readonly IEventJournal m_journal;
        private readonly IEventStreamConsumingSessionFactory m_sessionFactory;
        private readonly IEventMutationPipelineFactory m_pipelineFactory;
        private readonly INotificationHub m_notificationHub;
        private readonly INotificationHubController m_notificationHubController;
        private readonly EventStreamConnectivityState m_connectivityState;

        public EventStoreConnection(
            IEventJournal journal,
            INotificationPipelineFactory notificationPipelineFactory,
            IEventStreamConsumingSessionFactory sessionFactory,
            IEventMutationPipelineFactory pipelineFactory)
        {
            Require.NotNull(journal, "journal");
            Require.NotNull(notificationPipelineFactory, "notificationPipelineFactory");
            Require.NotNull(sessionFactory, "sessionFactory");
            Require.NotNull(pipelineFactory, "pipelineFactory");

            m_journal = journal;
            m_sessionFactory = sessionFactory;
            m_pipelineFactory = pipelineFactory;

            m_notificationHubController = notificationPipelineFactory.CreateHubController();
            m_notificationHub = notificationPipelineFactory.CreateHub();
            m_notificationHubController.StartHub(m_notificationHub);
            m_connectivityState = new EventStreamConnectivityState();
        }

        public async Task<IEventStreamReader> CreateStreamReaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectivityState.EnsureConnectionIsActive();

            var reader = new EventStreamReader(
                streamName: streamName,
                connectivityState: m_connectivityState,
                streamCursor: await m_journal.OpenEventStreamCursorAsync(streamName),
                mutationPipeline: m_pipelineFactory.CreateIncomingPipeline(),
                openCursor: version => m_journal.OpenEventStreamCursorAsync(streamName, version));

            return reader;
        }

        public async Task<IEventStreamReader> CreateStreamReaderAsync(string streamName, StreamVersion streamVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectivityState.EnsureConnectionIsActive();

            var reader = new EventStreamReader(
                streamName: streamName,
                connectivityState: m_connectivityState,
                streamCursor: await m_journal.OpenEventStreamCursorAsync(streamName, streamVersion),
                mutationPipeline: m_pipelineFactory.CreateIncomingPipeline(),
                openCursor: version => m_journal.OpenEventStreamCursorAsync(streamName, version));

            return reader;
        }

        public async Task<IEventStreamWriter> CreateStreamWriterAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectivityState.EnsureConnectionIsActive();

            var endOfStream = await m_journal.ReadEndOfStreamPositionAsync(streamName);

            return new EventStreamWriter(
                streamName: streamName,
                endOfStream: endOfStream,
                connectivityState: m_connectivityState,
                journal: m_journal,
                mutationPipeline: m_pipelineFactory.CreateOutgoingPipeline(),
                notificationHub: m_notificationHub);
        }

        public async Task<IEventStreamProducer> CreateStreamProducerAsync(string streamName)
        {
            m_connectivityState.EnsureConnectionIsActive();

            return new EventStreamProducer(
                streamWriter: await CreateStreamWriterAsync(streamName),
                retryPolicy: RetryPolicy.Default);
        }

        public async Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, string consumerName)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(consumerName, "consumerName");

            m_connectivityState.EnsureConnectionIsActive();

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

        public void Close()
        {
            m_connectivityState.ChangeToClosing();
            m_notificationHubController.StopHub(m_notificationHub);
            m_connectivityState.ChangeToClosed();
        }
    }
}
