using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public class EventStreamWriter : IEventStreamWriter
    {
        private readonly string m_streamName;
        private readonly IEventJournal m_journal;
        private readonly IEventSerializer m_serializer;

        private EventStreamPosition m_endOfStream;

        public EventStreamWriter(
            string streamName,
            EventStreamPosition endOfStream,
            IEventJournal journal,
            IEventSerializer serializer)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(journal, "journal");
            Require.NotNull(serializer, "serializer");

            m_streamName = streamName;
            m_endOfStream = endOfStream;
            m_journal = journal;
            m_serializer = serializer;
        }

        public async Task AppendEvents(IReadOnlyCollection<object> events)
        {
            Require.NotNull(events, "events");

            if (events.Count == 0)
            {
                return;
            }

            var journaledEvents = new List<JournaledEvent>(events.Count);
            foreach (var eventObject in events)
            {
                journaledEvents.Add(
                    JournaledEvent.Create(
                        Guid.NewGuid(),
                        eventObject,
                        (obj, type, writer) => m_serializer.Serialize(obj, type, writer)));
            }

            m_endOfStream = await m_journal.AppendEventsAsync(m_streamName, m_endOfStream, journaledEvents);
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
