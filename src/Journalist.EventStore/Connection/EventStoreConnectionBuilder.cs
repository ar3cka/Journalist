using System;
using Journalist.EventStore.Configuration;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Streams;
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

            var journalTable = m_factory.CreateTable(
                m_configuration.StorageConnectionString,
                m_configuration.JournalTableName);

            var journalMetadataTable = m_factory.CreateTable(
                m_configuration.StorageConnectionString,
                m_configuration.JournalMetadataTableName);

            var sessionFactory = new EventStreamConsumingSessionFactory(
                m_factory.CreateBlobContainer(
                    m_configuration.StorageConnectionString,
                    m_configuration.StreamConsumerSessionsBlobName));

            var pipelineFactory = new EventMutationPipelineFactory(
                m_configuration.IncomingMessageMutators,
                m_configuration.OutgoingMessageMutators);

            var notificationQueue = m_factory.CreateQueue(
                m_configuration.StorageConnectionString,
                m_configuration.NotificationQueueName);

            var notificationPipelineFactory = new NotificationPipelineFactory(
                new NotificationsChannel(notificationQueue),
                m_configuration.NotificationListeners);

            return new EventStoreConnection(
                new EventJournal(journalTable),
                new EventStreamConsumersRegistry(journalMetadataTable),
                notificationPipelineFactory,
                sessionFactory,
                pipelineFactory);
        }
    }
}
