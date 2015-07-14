using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Streams
{
    public class EventStreamWriter : IEventStreamWriter
    {
        private readonly string m_streamName;
        private readonly IEventJournal m_journal;
        private readonly IEventMutationPipeline m_mutationPipeline;
        private readonly INotificationHub m_notificationHub;

        private EventStreamPosition m_endOfStream;

        public EventStreamWriter(
            string streamName,
            EventStreamPosition endOfStream,
            IEventJournal journal,
            IEventMutationPipeline mutationPipeline,
            INotificationHub notificationHub)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(journal, "journal");
            Require.NotNull(mutationPipeline, "mutationPipeline");
            Require.NotNull(notificationHub, "notificationHub");

            m_streamName = streamName;
            m_endOfStream = endOfStream;
            m_journal = journal;
            m_mutationPipeline = mutationPipeline;
            m_notificationHub = notificationHub;
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

            var fromVersion = m_endOfStream.Version;
            m_endOfStream = await m_journal.AppendEventsAsync(m_streamName, m_endOfStream, mutatedEvents);

            await m_notificationHub.NotifyAsync(new EventStreamUpdated(
                m_streamName,
                fromVersion,
                m_endOfStream.Version));
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
