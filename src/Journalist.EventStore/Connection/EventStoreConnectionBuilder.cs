using System;
using System.Linq;
using Journalist.EventStore.Configuration;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Processing;
using Journalist.EventStore.Streams;
using Journalist.EventStore.Utils.Polling;
using Journalist.WindowsAzure.Storage;

namespace Journalist.EventStore.Connection
{
    public class EventStoreConnectionBuilder
    {
        private readonly Action<IEventStoreConnectionConfiguration> m_configure;

        private EventStoreConnectionConfiguration m_configuration;
        private StorageFactory m_factory;

        private EventStoreConnectionBuilder(Action<IEventStoreConnectionConfiguration> configure)
        {
            Require.NotNull(configure, "configure");

            m_configure = configure;
        }

        public static EventStoreConnectionBuilder Create(Action<IEventStoreConnectionConfiguration> configure)
        {
            return new EventStoreConnectionBuilder(configure);
        }

        public IEventStoreConnection Build()
        {
            if (m_configuration == null)
            {
                m_configuration = new EventStoreConnectionConfiguration();
                m_configure(m_configuration);

                m_configuration.AssertConfigurationCompleted();
                m_factory = new StorageFactory();
            }

            var connectivityState = new EventStoreConnectionState();

            var journalTable = new EventJournalTable(m_factory.CreateTable(
                m_configuration.StorageConnectionString,
                m_configuration.JournalTableName));

            var deploymentTable = m_factory.CreateTable(
                m_configuration.StorageConnectionString,
                m_configuration.EventStoreDeploymentTableName);

            var sessionFactory = new EventStreamConsumingSessionFactory(
                m_factory.CreateBlobContainer(
                    m_configuration.StorageConnectionString,
                    m_configuration.StreamConsumerSessionsBlobContainerName));

            var pipelineFactory = new EventMutationPipelineFactory(
                m_configuration.IncomingMessageMutators,
                m_configuration.OutgoingMessageMutators);

            var queues = Enumerable
                .Range(0, m_configuration.NotificationQueuePartitionCount)
                .Select(index => m_factory.CreateQueue(
                    m_configuration.StorageConnectionString,
                    m_configuration.NotificationQueueName + "-" + index.ToString("D3")))
                .ToArray();

			var eventJournal = new EventJournal(journalTable);

			var consumersRegistry = new EventStreamConsumers(deploymentTable);
			var consumersService = new ConsumersService(consumersRegistry, eventJournal);

            var notificationHub = new NotificationHub(
                new PollingJob("NotificationHubPollingJob", new PollingTimeout()),
                new NotificationsChannel(queues, new NotificationFormatter()),
                new ReceivedNotificationProcessor());

            var pendingNotificationTable = m_factory.CreateTable(m_configuration.StorageConnectionString, m_configuration.PendingNotificationsTableName);
            var pendingNotifications = new PendingNotifications(pendingNotificationTable);
            var pendingNotificationsChaserTimeout = new PollingTimeout(
                TimeSpan.FromMinutes(Constants.Settings.PENDING_NOTIFICATIONS_CHASER_INITIAL_TIMEOUT_IN_MINUTES),
                Constants.Settings.PENDING_NOTIFICATIONS_CHASER_TIMEOUT_MULTIPLIER,
                Constants.Settings.PENDING_NOTIFICATIONS_CHASER_TIMEOUT_INCREASING_THRESHOLD,
                TimeSpan.FromMinutes(Constants.Settings.PENDING_NOTIFICATIONS_CHASER_MAX_TIMEOUT_IN_MINUTES));

            var pendingNotificationsChaser = new PendingNotificationsChaser(
                pendingNotifications,
                notificationHub,
                new PollingJob("PendingNotificationsChaserPollingJob", pendingNotificationsChaserTimeout),
                m_factory.CreateBlobContainer(
                    m_configuration.StorageConnectionString,
                    m_configuration.PendingNotificationsChaserExclusiveAccessLockBlobContainerName).CreateBlockBlob(
                        m_configuration.PendingNotificationsChaserExclusiveAccessLockBlobName));

            connectivityState.ConnectionCreated += (sender, args) =>
            {
                if (m_configuration.BackgroundProcessingEnabled)
                {
                    foreach (var notificationListener in m_configuration.NotificationListeners)
                    {
                        notificationHub.Subscribe(notificationListener);
                    }

                    notificationHub.StartNotificationProcessing(args.Connection);
                    pendingNotificationsChaser.Start();
                }
            };

            connectivityState.ConnectionClosing += (sender, args) =>
            {
                if (m_configuration.BackgroundProcessingEnabled)
                {
                    notificationHub.StopNotificationProcessing();
                    pendingNotificationsChaser.Stop();
                }
            };

            connectivityState.ConnectionClosed += (sender, args) =>
            {
                if (m_configuration.BackgroundProcessingEnabled)
                {
                    foreach (var notificationListener in m_configuration.NotificationListeners)
                    {
                        notificationHub.Unsubscribe(notificationListener);
                    }
                }
            };

	        return new EventStoreConnection(
                connectivityState,
                eventJournal,
                notificationHub,
                pendingNotifications,
                consumersRegistry,
                sessionFactory,
                pipelineFactory,
				consumersService);
        }
    }
}
