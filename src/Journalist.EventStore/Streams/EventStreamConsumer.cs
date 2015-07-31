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
        private readonly Func<StreamVersion, Task> m_commitConsumedVersion;

        private bool m_hasUnprocessedEvents;
        private bool m_consuming;
        private bool m_receiving;
        private int m_eventSliceOffset;
        private int m_commitedEventCount;
        private StreamVersion m_commitedStreamVersion;
        private bool m_closed;

        public EventStreamConsumer(
            EventStreamConsumerId consumerId,
            IEventStreamReader streamReader,
            IEventStreamConsumingSession session,
            StreamVersion commitedStreamVersion,
            Func<StreamVersion, Task> commitConsumedVersion)
        {
            Require.NotNull(consumerId, "consumerId");
            Require.NotNull(streamReader, "streamReader");
            Require.NotNull(session, "session");
            Require.NotNull(commitConsumedVersion, "commitConsumedVersion");

            m_reader = streamReader;
            m_session = session;
            m_commitConsumedVersion = commitConsumedVersion;
            m_commitedStreamVersion = commitedStreamVersion;
        }

        public async Task<bool> ReceiveEventsAsync()
        {
            AssertConsumerWasNotClosed();
            AssertConsumerIsNotInConsumingState();

            if (await m_session.PromoteToLeaderAsync())
            {
                await CommitReceivedStreamVersionAsync();
                await ReceiveEventsFromReaderAsync();

                if (m_hasUnprocessedEvents)
                {
                    return true;
                }

                await CompletedReceivingAsync();
            }

            return false;
        }

        public async Task CommitProcessedStreamVersionAsync(bool skipCurrent)
        {
            AssertConsumerWasNotClosed();

            if (m_consuming)
            {
                var eventOffset = skipCurrent ? m_eventSliceOffset : m_eventSliceOffset + 1;
                if (eventOffset <= m_commitedEventCount)
                {
                    // current event has been commited.
                    return;
                }

                var version = m_commitedStreamVersion.Increment(eventOffset - m_commitedEventCount);

                if (m_commitedStreamVersion < version)
                {
                    await m_commitConsumedVersion(version);

                    m_commitedStreamVersion = version;
                    m_commitedEventCount++;
                }
            }
            else
            {
                await CommitReceivedStreamVersionAsync();
            }
        }

        public async Task CloseAsync()
        {
            AssertConsumerWasNotClosed();

            if (m_receiving)
            {
                if (m_hasUnprocessedEvents)
                {
                    if (m_consuming)
                    {
                        await CommitProcessedStreamVersionAsync(true);
                    }
                }
                else
                {
                    await CommitProcessedStreamVersionAsync(false);
                }
            }

            await CompletedReceivingAsync();

            m_closed = true;
        }

        public IEnumerable<JournaledEvent> EnumerateEvents()
        {
            AssertConsumerWasNotClosed();
            AssertConsumerIsNotInConsumingState();

            if (m_hasUnprocessedEvents)
            {
                m_consuming = true;

                for (m_eventSliceOffset = 0; m_eventSliceOffset < m_reader.Events.Count; m_eventSliceOffset++)
                {
                    yield return m_reader.Events[m_eventSliceOffset];
                }

                m_consuming = false;
                m_hasUnprocessedEvents = false;

                yield break;
            }

            throw new InvalidOperationException("Consumer stream is empty.");
        }

        public string StreamName
        {
            get { return m_reader.StreamName; }
        }

        private async Task CommitReceivedStreamVersionAsync()
        {
            if (m_receiving && m_commitedStreamVersion != m_reader.StreamVersion)
            {
                await m_commitConsumedVersion(m_reader.StreamVersion);
                m_commitedStreamVersion = m_reader.StreamVersion;
                m_eventSliceOffset = 0;
            }
        }

        private async Task ReceiveEventsFromReaderAsync()
        {
            if (m_reader.IsCompleted)
            {
                await m_reader.ContinueAsync();
                m_receiving = false;
            }

            if (m_reader.HasEvents)
            {
                await m_reader.ReadEventsAsync();

                m_hasUnprocessedEvents = true;
                m_receiving = true;
            }
        }

        private async Task CompletedReceivingAsync()
        {
            await m_session.FreeAsync();
            m_receiving = false;
        }

        private void AssertConsumerIsNotInConsumingState()
        {
            if (m_consuming)
            {
                throw new InvalidOperationException("Consumer stream is already opened.");
            }
        }

        private void AssertConsumerWasNotClosed()
        {
            if (m_closed)
            {
                throw new InvalidOperationException("Consumer was closed.");
            }
        }
    }
}
