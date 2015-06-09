using System.Threading.Tasks;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public class EventStream : IEventStream
    {
        private readonly IEventJournal m_journal;

        public EventStream(IEventJournal journal)
        {
            Require.NotNull(journal, "journal");

            m_journal = journal;
        }

        public Task<IEventStreamReader> OpenReaderAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            throw new System.NotImplementedException();
        }

        public Task<IEventStreamWriter> OpenWriterAsync(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            throw new System.NotImplementedException();
        }
    }
}
