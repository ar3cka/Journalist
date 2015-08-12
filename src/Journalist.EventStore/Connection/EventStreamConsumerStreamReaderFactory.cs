using System.Threading.Tasks;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public class EventStreamConsumerStreamReaderFactory : IEventStreamConsumerStreamReaderFactory
    {
        private readonly EventStreamReaderId m_readerId;
        private readonly IEventJournal m_journal;
        private readonly IEventJournalReaders m_journalReaders;
        private readonly IEventStoreConnectionState m_connectionState;
        private readonly IEventMutationPipeline m_mutationPipeline;
        private readonly EventStreamConsumerConfiguration m_configuration;

        public EventStreamConsumerStreamReaderFactory(
            EventStreamReaderId readerId,
            IEventJournal journal,
            IEventJournalReaders journalReaders,
            IEventStoreConnectionState connectionState,
            IEventMutationPipeline mutationPipeline,
            EventStreamConsumerConfiguration configuration)
        {
            Require.NotNull(readerId, "readerId");
            Require.NotNull(journal, "journal");
            Require.NotNull(journalReaders, "journalReaders");
            Require.NotNull(connectionState, "connectionState");
            Require.NotNull(mutationPipeline, "mutationPipeline");
            Require.NotNull(configuration, "configuration");

            m_readerId = readerId;
            m_journal = journal;
            m_journalReaders = journalReaders;
            m_connectionState = connectionState;
            m_mutationPipeline = mutationPipeline;
            m_configuration = configuration;
        }

        public async Task<IEventStreamReader> CreateAsync()
        {
            if (await m_journalReaders.IsRegisteredAsync(m_readerId))
            {
                return await CreateReaderAsync(m_configuration.StreamName, m_readerId);
            }

            await m_journalReaders.RegisterAsync(m_readerId);
            if (m_configuration.StartReadingStreamFromEnd)
            {
                var endOfStream = await m_journal.ReadEndOfStreamPositionAsync(m_configuration.StreamName);
                await m_journal.CommitStreamReaderPositionAsync(m_configuration.StreamName, m_readerId, endOfStream.Version);
            }
            else
            {
                await m_journal.CommitStreamReaderPositionAsync(m_configuration.StreamName, m_readerId, StreamVersion.Unknown);
            }

            return await CreateReaderAsync(m_configuration.StreamName, m_readerId);
        }

        private async Task<IEventStreamReader> CreateReaderAsync(string streamName, EventStreamReaderId readerId)
        {
            return new EventStreamReader(
                m_configuration.StreamName,
                m_connectionState,
                await m_journal.OpenEventStreamCursorAsync(
                    streamName,
                    readerId,
                    Constants.Settings.DEFAULT_EVENT_SLICE_SIZE),
                m_mutationPipeline);
        }
    }
}
