using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public class EventStreamWriter : IEventStreamWriter
    {
        private readonly string m_streamName;
        private readonly IEventJournal m_journal;

        private EventStreamPosition m_endOfStream;

        public EventStreamWriter(
            string streamName,
            EventStreamPosition endOfStream,
            IEventJournal journal)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(journal, "journal");

            m_streamName = streamName;
            m_endOfStream = endOfStream;
            m_journal = journal;
        }

        public async Task AppendEvents(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            if (events.Count == 0)
            {
                return;
            }

            m_endOfStream = await m_journal.AppendEventsAsync(m_streamName, m_endOfStream, events);
        }

        public int StreamPosition
        {
            get { return (int)m_endOfStream.Version; }
        }

        public string StreamName
        {
            get { return m_streamName; }
        }
    }
}
