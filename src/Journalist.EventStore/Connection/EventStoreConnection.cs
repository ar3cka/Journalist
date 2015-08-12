using System;
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
        private readonly IEventStoreConnectionState m_connectionState;

        public EventStoreConnection(
            IEventStoreConnectionState connectionState,
            IEventJournal journal,
            INotificationHub notificationHub,
            IEventStreamConsumersRegistry consumersRegistry,
            IEventStreamConsumingSessionFactory sessionFactory,
            IEventMutationPipelineFactory pipelineFactory)
        {
            Require.NotNull(connectionState, "connectionState");
            Require.NotNull(journal, "journal");
            Require.NotNull(notificationHub, "notificationHub");
            Require.NotNull(consumersRegistry, "consumersRegistry");
            Require.NotNull(sessionFactory, "sessionFactory");
            Require.NotNull(pipelineFactory, "pipelineFactory");

            m_connectionState = connectionState;
            m_journal = journal;
            m_notificationHub = notificationHub;
            m_consumersRegistry = consumersRegistry;
            m_sessionFactory = sessionFactory;
            m_pipelineFactory = pipelineFactory;

            m_connectionState.ChangeToCreated(this);
        }

        public async Task<IEventStreamReader> CreateStreamReaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectionState.EnsureConnectionIsActive();

            var reader = new EventStreamReader(
                streamName: streamName,
                connectionState: m_connectionState,
                streamCursor: await m_journal.OpenEventStreamCursorAsync(streamName),
                mutationPipeline: m_pipelineFactory.CreateIncomingPipeline());

            return reader;
        }

        public async Task<IEventStreamReader> CreateStreamReaderAsync(string streamName, StreamVersion streamVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectionState.EnsureConnectionIsActive();

            var reader = new EventStreamReader(
                streamName: streamName,
                connectionState: m_connectionState,
                streamCursor: await m_journal.OpenEventStreamCursorAsync(streamName, streamVersion),
                mutationPipeline: m_pipelineFactory.CreateIncomingPipeline());

            return reader;
        }

        public async Task<IEventStreamWriter> CreateStreamWriterAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectionState.EnsureConnectionIsActive();

            var endOfStream = await m_journal.ReadEndOfStreamPositionAsync(streamName);

            return new EventStreamWriter(
                streamName: streamName,
                connectionState: m_connectionState,
                endOfStream: endOfStream,
                journal: m_journal,
                mutationPipeline: m_pipelineFactory.CreateOutgoingPipeline(),
                notificationHub: m_notificationHub);
        }

        public async Task<IEventStreamProducer> CreateStreamProducerAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            m_connectionState.EnsureConnectionIsActive();

            return new EventStreamProducer(
                streamWriter: await CreateStreamWriterAsync(streamName),
                retryPolicy: RetryPolicy.Default);
        }

        public async Task<IEventStreamConsumer> CreateStreamConsumerAsync(Action<IEventStreamConsumerConfiguration> configure)
        {
            Require.NotNull(configure, "configure");

            var configuration = new EventStreamConsumerConfiguration();
            configure(configuration);
            configuration.AsserConfigurationCompleted();

            m_connectionState.EnsureConnectionIsActive();

            var readerId = await EnsureReaderIsRegistered(configuration);

            var streamPosition = await m_journal.ReadEndOfStreamPositionAsync(configuration.StreamName);
            var readerVersion = await m_journal.ReadStreamReaderPositionAsync(configuration.StreamName, readerId);

            var session = m_sessionFactory.CreateSession(
                consumerId: readerId,
                streamName: configuration.StreamName);

            Func<StreamVersion, Task> commitReaderVersion = currentVersion => m_journal.CommitStreamReaderPositionAsync(
                streamName: configuration.StreamName,
                readerId: readerId,
                version: currentVersion);

            return new EventStreamConsumer(
                consumerId: readerId,
                session: session,
                readerFactory: new EventStreamConsumerStreamReaderFactory(
                    connection: this,
                    streamName: configuration.StreamName,
                    startReadingFromTheEnd: configuration.StartReadingStreamFromEnd,
                    readerStreamVersion: readerVersion,
                    streamVersion: streamPosition.Version,
                    commitReaderVersion: commitReaderVersion),
                stateMachine: new EventStreamConsumerStateMachine(readerVersion),
                autoCommitProcessedStreamVersion: configuration.UseAutoCommitProcessedStreamPositionBehavior,
                commitConsumedVersion: commitReaderVersion);
        }

        public Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, string consumerName)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(consumerName, "consumerName");

            return CreateStreamConsumerAsync(config => config
                .ReadStream(streamName)
                .UseConsumerName(consumerName));
        }

        public Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, EventStreamReaderId consumerId)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(consumerId, "consumerId");

            return CreateStreamConsumerAsync(config => config
                .ReadStream(streamName)
                .UseConsumerId(consumerId));
        }

        public void Close()
        {
            m_connectionState.ChangeToClosing();
            m_connectionState.ChangeToClosed();
        }

        private async Task<EventStreamReaderId> EnsureReaderIsRegistered(EventStreamConsumerConfiguration configuration)
        {
            EventStreamReaderId consumerId;
            if (configuration.ConsumerRegistrationRequired)
            {
                consumerId = await m_consumersRegistry.RegisterAsync(configuration.ConsumerName);
            }
            else
            {
                consumerId = configuration.ConsumerId;

                if (!(await m_consumersRegistry.IsResistedAsync(consumerId)))
                {
                    throw new UnknownEventStreamConsumerException(consumerId);
                }
            }
            return consumerId;
        }
    }
}
