using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Streams
{
    public class EventStreamWriter : EventStreamInteractionEntity, IEventStreamWriter
    {
        private readonly IEventJournal m_journal;
        private readonly IEventMutationPipeline m_mutationPipeline;
        private readonly INotificationHub m_notificationHub;
        private readonly IPendingNotifications m_pendingNotification;

        private EventStreamHeader m_endOfStream;

        public EventStreamWriter(
            string streamName,
            IEventStoreConnectionState connectionState,
            EventStreamHeader endOfStream,
            IEventJournal journal,
            IEventMutationPipeline mutationPipeline,
            INotificationHub notificationHub,
            IPendingNotifications pendingNotification) : base(streamName, connectionState)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(journal, "journal");
            Require.NotNull(mutationPipeline, "mutationPipeline");
            Require.NotNull(notificationHub, "notificationHub");
            Require.NotNull(pendingNotification, "pendingNotification");

            m_endOfStream = endOfStream;
            m_journal = journal;
            m_mutationPipeline = mutationPipeline;
            m_notificationHub = notificationHub;
            m_pendingNotification = pendingNotification;
        }

        public async Task AppendEventsAsync(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            ConnectionState.EnsureConnectionIsActive();

            if (events.Count == 0)
            {
                return;
            }

            var fromVersion = m_endOfStream.Version;
            var mutatedEvents = new List<JournaledEvent>(events.Count);
            mutatedEvents.AddRange(events.Select(journaledEvent => m_mutationPipeline.Mutate(journaledEvent)));

            await m_pendingNotification.AddAsync(StreamName, fromVersion, mutatedEvents.Count);
            m_endOfStream = await m_journal.AppendEventsAsync(StreamName, m_endOfStream, mutatedEvents);
            await m_notificationHub.NotifyAsync(new EventStreamUpdated(StreamName, fromVersion, m_endOfStream.Version));
            await m_pendingNotification.DeleteAsync(StreamName, fromVersion);
        }

        public async Task MoveToEndOfStreamAsync()
        {
            ConnectionState.EnsureConnectionIsActive();

            m_endOfStream = await m_journal.ReadStreamHeaderAsync(StreamName);
        }

        public override EventStreamHeader StreamHeader
        {
            get { return m_endOfStream; }
        }
    }
}
