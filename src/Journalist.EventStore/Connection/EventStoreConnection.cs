using System;
using System.Collections.Generic;
using System.Linq;
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
            IEventMutationPipelineFactory pipelineFactory, 
            IFailedNotificationsStore failedNotificationsStore)
        {
            Require.NotNull(connectionState, nameof(connectionState));
            Require.NotNull(journal, nameof(journal));
            Require.NotNull(notificationHub, nameof(notificationHub));
            Require.NotNull(pendingNotifications, nameof(pendingNotifications));
            Require.NotNull(consumers, nameof(consumers));
            Require.NotNull(sessionFactory, nameof(sessionFactory));
            Require.NotNull(pipelineFactory, nameof(pipelineFactory));
            Require.NotNull(failedNotificationsStore, nameof(failedNotificationsStore));

            m_connectionState = connectionState;
            m_journal = journal;
            m_notificationHub = notificationHub;
            m_pendingNotifications = pendingNotifications;
            m_consumers = consumers;
            m_sessionFactory = sessionFactory;
            m_pipelineFactory = pipelineFactory;
            FailedNotificationsStore = failedNotificationsStore;

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
            configuration.AssertConfigurationCompleted();

            m_connectionState.EnsureConnectionIsActive();

            var readerId = await m_consumers.RegisterAsync(configuration.ConsumerName);
            return CreateStreamConsumer(readerId, configuration);
        }

        public Task<IEventStreamConsumer> CreateStreamConsumerAsync(string streamName, string consumerName)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotEmpty(consumerName, "consumerName");

            return CreateStreamConsumerAsync(config => config
                .ReadStream(streamName)
                .WithName(consumerName));
        }

        public async Task<IEnumerable<IEventStreamConsumer>> EnumerateConsumersAsync(string streamName)
        {
            Require.NotEmpty(streamName, nameof(streamName));

            var consumerIds = await m_consumers.EnumerateAsync();

            return consumerIds.Select(consumerId =>
            {
                var configuration = new EventStreamConsumerConfiguration();
                configuration.ReadStream(streamName);
                return CreateStreamConsumer(consumerId, configuration);
            });
        }

        public IFailedNotificationsStore FailedNotificationsStore { get; }

        public void Close()
        {
            m_connectionState.ChangeToClosing();
            m_connectionState.ChangeToClosed();
        }

        private IEventStreamConsumer CreateStreamConsumer(EventStreamReaderId readerId, EventStreamConsumerConfiguration configuration)
        {
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
    }
}
