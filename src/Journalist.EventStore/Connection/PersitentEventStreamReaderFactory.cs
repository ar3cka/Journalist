using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public class PersistentEventStreamReaderFactory : IEventStreamReaderFactory
    {
        private readonly EventStreamReaderId m_readerId;
        private readonly IEventJournal m_journal;
        private readonly IEventStoreConnectionState m_connectionState;
        private readonly IEventMutationPipeline m_mutationPipeline;
        private readonly EventStreamConsumerConfiguration m_configuration;

        public PersistentEventStreamReaderFactory(
            EventStreamReaderId readerId,
            IEventJournal journal,
            IEventStoreConnectionState connectionState,
            IEventMutationPipeline mutationPipeline,
            EventStreamConsumerConfiguration configuration)
        {
            Require.NotNull(readerId, "readerId");
            Require.NotNull(journal, "journal");
            Require.NotNull(connectionState, "connectionState");
            Require.NotNull(mutationPipeline, "mutationPipeline");
            Require.NotNull(configuration, "configuration");

            m_readerId = readerId;
            m_journal = journal;
            m_connectionState = connectionState;
            m_mutationPipeline = mutationPipeline;
            m_configuration = configuration;
        }

        public async Task<IEventStreamReader> CreateAsync()
        {
            var cursor = await m_journal.OpenEventStreamCursorAsync(
                m_configuration.StreamName,
                m_readerId,
                Constants.Settings.EVENT_SLICE_SIZE);

            return new EventStreamReader(
                m_configuration.StreamName,
                m_connectionState,
                cursor,
                m_mutationPipeline);
        }
    }
}
