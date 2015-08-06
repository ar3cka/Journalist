using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumer : IEventStreamConsumer
    {
        private readonly IEventStreamReader m_reader;
        private readonly IEventStreamConsumingSession m_session;
        private readonly bool m_autoCommitProcessedStreamVersion;
        private readonly Func<StreamVersion, Task> m_commitConsumedVersion;
        private readonly EventStreamConsumerStateMachine m_stm;

        private int m_eventSliceOffset;

        public EventStreamConsumer(
            EventStreamConsumerId consumerId,
            IEventStreamReader streamReader,
            IEventStreamConsumingSession session,
            bool autoCommitProcessedStreamVersion,
            StreamVersion commitedStreamVersion,
            Func<StreamVersion, Task> commitConsumedVersion)
        {
            Require.NotNull(consumerId, "consumerId");
            Require.NotNull(streamReader, "streamReader");
            Require.NotNull(session, "session");
            Require.NotNull(commitConsumedVersion, "commitConsumedVersion");

            m_reader = streamReader;
            m_session = session;
            m_autoCommitProcessedStreamVersion = autoCommitProcessedStreamVersion;
            m_commitConsumedVersion = commitConsumedVersion;
            m_stm = new EventStreamConsumerStateMachine(commitedStreamVersion);
        }

        public async Task<bool> ReceiveEventsAsync()
        {
            m_stm.ReceivingStarted();

            if (await m_session.PromoteToLeaderAsync())
            {
                if (m_stm.CommitRequired(m_autoCommitProcessedStreamVersion))
                {
                    var version = m_stm.CalculateConsumedStreamVersion(false);
                    m_commitConsumedVersion(version);
                }

                await ReceiveEventsFromReaderAsync();

                if (m_stm.ReceivingTerminationRequired)
                {
                    await CompletedReceivingAsync();
                    return false;
                }

                return true;
            }

            return false;
        }

        public async Task CommitProcessedStreamVersionAsync(bool skipCurrent)
        {
            var version = m_stm.CalculateConsumedStreamVersion(skipCurrent);
            if (m_stm.CommitedStreamVersion < version)
            {
                await m_commitConsumedVersion(version);

                m_stm.ConsumedStreamVersionCommited(version);
            }
        }

        public async Task CloseAsync()
        {
            m_stm.ConsumerClosed();

            if (m_stm.CommitRequired(m_autoCommitProcessedStreamVersion))
            {
                await CommitProcessedStreamVersionAsync(true);
            }

            await CompletedReceivingAsync();
        }

        public IEnumerable<JournaledEvent> EnumerateEvents()
        {
            m_stm.ConsumingStarted();

            for (m_eventSliceOffset = 0; m_eventSliceOffset < m_reader.Events.Count; m_eventSliceOffset++)
            {
                m_stm.EventProcessingStarted();

                yield return m_reader.Events[m_eventSliceOffset];
            }

            m_stm.ConsumingCompleted();
        }

        public string StreamName
        {
            get { return m_reader.StreamName; }
        }

        private async Task ReceiveEventsFromReaderAsync()
        {
            if (m_reader.IsCompleted)
            {
                await m_reader.ContinueAsync();
            }

            if (m_reader.HasEvents)
            {
                await m_reader.ReadEventsAsync();

                m_stm.ReceivingCompleted(m_reader.Events.Count);
            }
        }

        private async Task CompletedReceivingAsync()
        {
            await m_session.FreeAsync();
        }
    }
}
