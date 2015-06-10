using System;
using Journalist.EventStore.Journal;
using Journalist.WindowsAzure.Storage;

namespace Journalist.EventStore.Streams.Configuration
{
    public class EventStreamBuilder
    {
        private readonly Action<IEventStreamConfiguration> m_configure;

        private EventStreamConfiguration m_configuration;
        private StorageFactory m_factory;

        private EventStreamBuilder(Action<IEventStreamConfiguration> configure)
        {
            Require.NotNull(configure, "configure");

            m_configure = configure;
        }

        public static EventStreamBuilder Create(Action<IEventStreamConfiguration> configure)
        {
            return new EventStreamBuilder(configure);
        }

        public IEventStream Build()
        {
            if (m_configuration == null)
            {
                m_configuration = new EventStreamConfiguration();
                m_configure(m_configuration);

                m_configuration.AssertConfigurationCompleted();
                m_factory = new StorageFactory();
            }

            var journalTable = m_factory.CreateTable(
                m_configuration.StorageConnectionString,
                m_configuration.JournalTableName);

            return new EventStream(
                new EventJournal(journalTable),
                m_configuration.EventSerializer);
        }
    }
}
