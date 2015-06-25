using System;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;
using Journalist.WindowsAzure.Storage;

namespace Journalist.EventStore.Configuration
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

            var sessionsBlob = m_factory.CreateBlobContainer(
                m_configuration.StorageConnectionString,
                m_configuration.StreamConsumerSessionsBlobName);

            return new EventStoreConnection(
                new EventJournal(journalTable),
                new EventStreamConsumingSessionFactory(sessionsBlob));
        }
    }
}
