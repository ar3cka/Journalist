using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumer : IEventStreamConsumer
    {
        private readonly IEventStreamConsumingSession m_session;
        private readonly IEventStreamReaderFactory m_readerFactory;
        private readonly bool m_autoCommitProcessedStreamVersion;
        private readonly Func<StreamVersion, Task> m_commitConsumedVersion;
        private IEventStreamConsumerStateMachine m_stateMachine;
        private IEventStreamReader m_reader;

        public EventStreamConsumer(
            IEventStreamConsumingSession session,
            IEventStreamReaderFactory readerFactory,
            bool autoCommitProcessedStreamVersion,
            Func<StreamVersion, Task> commitConsumedVersion)
        {
            Require.NotNull(session, "session");
            Require.NotNull(readerFactory, "readerFactory");
            Require.NotNull(commitConsumedVersion, "commitConsumedVersion");

            m_session = session;
            m_readerFactory = readerFactory;
            m_autoCommitProcessedStreamVersion = autoCommitProcessedStreamVersion;
            m_commitConsumedVersion = commitConsumedVersion;
        }

        public async Task<ReceivingResultCode> ReceiveEventsAsync()
        {
            if (await m_session.PromoteToLeaderAsync())
            {
                await EnsureStreamReaderInitializedAsync();

                m_stateMachine.ReceivingStarted();

                if (m_stateMachine.CommitRequired(m_autoCommitProcessedStreamVersion))
                {
                    var version = m_stateMachine.CalculateConsumedStreamVersion(false);
                    await m_commitConsumedVersion(version);
                }

                if (m_reader.HasEvents)
                {
                    await m_reader.ReadEventsAsync();
                    m_stateMachine.ReceivingCompleted(m_reader.Events.Count);
                    return ReceivingResultCode.EventsReceived;

                }

                m_stateMachine.ReceivingCompleted(0);
                await m_session.FreeAsync();

                return ReceivingResultCode.EmptyStream;
            }

            return ReceivingResultCode.PromotionFailed;
        }

        public async Task CommitProcessedStreamVersionAsync(bool skipCurrent)
        {
            var version = m_stateMachine.CalculateConsumedStreamVersion(skipCurrent);
            if (m_stateMachine.CommitedStreamVersion < version)
            {
                await m_commitConsumedVersion(version);
                m_stateMachine.ConsumedStreamVersionCommited(version, skipCurrent);
            }
        }

        public Task CommitStreamVersionAsync(StreamVersion streamVersion)
        {
            Require.NotNull(streamVersion, "streamVersion");

            return m_commitConsumedVersion(streamVersion);
        }

        public async Task CloseAsync()
        {
            if (m_stateMachine == null)
            {
                return;
            }

            m_stateMachine.ConsumerClosed();

            if (m_stateMachine.CommitRequired(m_autoCommitProcessedStreamVersion))
            {
                await CommitProcessedStreamVersionAsync(true);
            }

            await m_session.FreeAsync();
        }

        public IEnumerable<JournaledEvent> EnumerateEvents()
        {
            Ensure.True(m_reader != null, "Reading was not started.");

            m_stateMachine.ConsumingStarted();

            for (var index = 0; index < m_reader.Events.Count; index++)
            {
                m_stateMachine.EventProcessingStarted();

                yield return m_reader.Events[index];
            }

            m_stateMachine.ConsumingCompleted();
        }

        private async Task EnsureStreamReaderInitializedAsync()
        {
            if (m_reader == null)
            {
                m_reader = await m_readerFactory.CreateAsync();

                m_stateMachine = m_reader.HasEvents
                    ? new EventStreamConsumerStateMachine(m_reader.ReaderStreamVersion.Decrement())
                    : new EventStreamConsumerStateMachine(m_reader.ReaderStreamVersion);
            }
        }

        public string StreamName => m_reader.StreamName;

        public StreamVersion CurrentConsumerVersion => m_stateMachine.CommitedStreamVersion;
    }
}
