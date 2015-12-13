using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Streams;
using Journalist.EventStore.Utils.RetryPolicies;

namespace Journalist.EventStore.Connection
{
    public class EventStoreConnection : IEventStoreConnection
    {
        private readonly IEventJournal m_journal;
        private readonly IEventStreamConsumers m_consumers;
        private readonly IEventStreamConsumingSessionFactory m_sessionFactory;
        private readonly IEventMutationPipelineFactory m_pipelineFactory;
        private readonly INotificationHub m_notificationHub;
        private readonly IPendingNotifications m_pendingNotifications;
        private readonly IEventStoreConnectionState m_connectionState;

        public EventStoreConnection(
            IEventStoreConnectionState connectionState,
            IEventJournal journal,
            INotificationHub notificationHub,
            IPendingNotifications pendingNotifications,
            IEventStreamConsumers consumers,
            IEventStreamConsumingSessionFactory sessionFactory,
            IEventMutationPipelineFactory pipelineFactory)
        {
            Require.NotNull(connectionState, "connectionState");
            Require.NotNull(journal, "journal");
            Require.NotNull(notificationHub, "notificationHub");
            Require.NotNull(pendingNotifications, "pendingNotifications");
            Require.NotNull(consumers, "consumers");
            Require.NotNull(sessionFactory, "sessionFactory");
            Require.NotNull(pipelineFactory, "pipelineFactory");

            m_connectionState = connectionState;
            m_journal = journal;
            m_notificationHub = notificationHub;
            m_pendingNotifications = pendingNotifications;
            m_consumers = consumers;
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

            var endOfStream = await m_journal.ReadStreamHeaderAsync(streamName);

            return new EventStreamWriter(
                streamName: streamName,
                connectionState: m_connectionState,
                endOfStream: endOfStream,
                journal: m_journal,
                mutationPipeline: m_pipelineFactory.CreateOutgoingPipeline(),
                notificationHub: m_notificationHub,
                pendingNotification: m_pendingNotifications);
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

            var readerId = await m_consumers.RegisterAsync(configuration.ConsumerName);
            var session = m_sessionFactory.CreateSession(readerId, configuration.StreamName);

            Func<StreamVersion, Task> commitReaderVersion = version => m_journal.CommitStreamReaderPositionAsync(
                streamName: configuration.StreamName,
                readerId: readerId,
                version: version);

            return new EventStreamConsumer(
                session: session,
                readerFactory: new PersistentEventStreamReaderFactory(
                    readerId,
                    m_journal,
                    m_connectionState,
                    m_pipelineFactory.CreateIncomingPipeline(),
                    configuration),
                autoCommitProcessedStreamVersion: configuration.UseAutoCommitProcessedStreamPositionBehavior,
                commitConsumedVersion: commitReaderVersion);
        }

        public Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, string consumerName)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(consumerName, "consumerName");

            return CreateStreamConsumerAsync(config => config
                .ReadStream(streamName)
                .WithName(consumerName));
        }

        public void Close()
        {
            m_connectionState.ChangeToClosing();
            m_connectionState.ChangeToClosed();
        }
    }
}
