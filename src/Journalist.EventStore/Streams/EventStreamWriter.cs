using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public class EventStreamWriter : IEventStreamWriter
    {
        private readonly string m_streamName;
        private readonly IEventJournal m_journal;
        private readonly IEventMutationPipeline m_mutationPipeline;

        private EventStreamPosition m_endOfStream;

        public EventStreamWriter(
            string streamName,
            EventStreamPosition endOfStream,
            IEventJournal journal, 
            IEventMutationPipeline mutationPipeline)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(journal, "journal");
            Require.NotNull(mutationPipeline, "mutationPipeline");

            m_streamName = streamName;
            m_endOfStream = endOfStream;
            m_journal = journal;
            m_mutationPipeline = mutationPipeline;
        }

        public async Task AppendEventsAsync(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            if (events.Count == 0)
            {
                return;
            }

            var mutatedEvents = new List<JournaledEvent>(events.Count);
            mutatedEvents.AddRange(events.Select(journaledEvent => m_mutationPipeline.Mutate(journaledEvent)));

            m_endOfStream = await m_journal.AppendEventsAsync(m_streamName, m_endOfStream, mutatedEvents);
        }

        public async Task MoveToEndOfStreamAsync()
        {
            m_endOfStream = await m_journal.ReadEndOfStreamPositionAsync(m_streamName);
        }

        public StreamVersion StreamVersion
        {
            get { return m_endOfStream.Version; }
        }

        public string StreamName
        {
            get { return m_streamName; }
        }
    }
}
