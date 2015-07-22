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
        private readonly IEventStreamConsumersRegistry m_consumersRegistry;
        private readonly IEventStreamConsumingSessionFactory m_sessionFactory;
        private readonly IEventMutationPipelineFactory m_pipelineFactory;
        private readonly INotificationHub m_notificationHub;
        private readonly INotificationHubController m_notificationHubController;
        private readonly EventStreamConnectivityState m_connectivityState;

        public EventStoreConnection(
            IEventJournal journal,
            IEventStreamConsumersRegistry consumersRegistry,
            INotificationPipelineFactory notificationPipelineFactory,
            IEventStreamConsumingSessionFactory sessionFactory,
            IEventMutationPipelineFactory pipelineFactory)
        {
            Require.NotNull(journal, "journal");
            Require.NotNull(consumersRegistry, "consumersRegistry");
            Require.NotNull(notificationPipelineFactory, "notificationPipelineFactory");
            Require.NotNull(sessionFactory, "sessionFactory");
            Require.NotNull(pipelineFactory, "pipelineFactory");

            m_journal = journal;
            m_consumersRegistry = consumersRegistry;
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
                connectivityState: m_connectivityState,
                endOfStream: endOfStream,
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

            var consumerId = await m_consumersRegistry.RegisterAsync(consumerName);

            var readerVersion = await m_journal.ReadStreamReaderPositionAsync(
                streamName: streamName,
                readerName: consumerId.ToString());

            var reader = await CreateStreamReaderAsync(
                streamName: streamName,
                streamVersion: readerVersion.Increment());

            var session = m_sessionFactory.CreateSession(
                consumerId: consumerId,
                streamName: streamName);

            return new EventStreamConsumer(
                consumerName: consumerName,
                consumerId: consumerId,
                streamReader: reader,
                session: session,
                commitedStreamVersion: readerVersion,
                commitConsumedVersion: currentVersion => m_journal.CommitStreamReaderPositionAsync(
                    streamName: streamName,
                    readerName: consumerId.ToString(),
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
