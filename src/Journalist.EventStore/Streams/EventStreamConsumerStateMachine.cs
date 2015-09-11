using System;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumerStateMachine : IEventStreamConsumerStateMachine
    {
        private abstract class State
        {
            public static readonly State Initial = new InitialState();
            public static readonly State Receiving = new ReceivingStartedState();
            public static readonly State Received = new ReceivingCompletedState();
            public static readonly State Consuming = new ConsumingState();
            public static readonly State Consumed = new ConsumedState();
            public static readonly State Closed = new ClosedState();

            public virtual State MoveToReceivingStartedState(EventStreamConsumerStateMachine stm)
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToReceivingCompletedState(EventStreamConsumerStateMachine stm, int eventsCount)
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToConsumingStarted()
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToConsumedState(EventStreamConsumerStateMachine stm)
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToClosedState(EventStreamConsumerStateMachine stm)
            {
                throw new NotImplementedException();
            }

            public virtual void EventProcessingStarted(EventStreamConsumerStateMachine stm)
            {
                throw new NotImplementedException();
            }

            public virtual StreamVersion CalculateConsumedStreamVersion(EventStreamConsumerStateMachine stm, bool skipCurrentEvent)
            {
                throw new NotImplementedException();
            }

            public virtual void ConsumedStreamVersionCommited(EventStreamConsumerStateMachine stm, StreamVersion version, bool skipCurrent)
            {
                stm.m_commitedVersion = version;
                stm.m_uncommittedEventCount = skipCurrent ? 0 : -1;
            }
        }

        private class InitialState : State
        {
            public override State MoveToReceivingStartedState(EventStreamConsumerStateMachine stm)
            {
                stm.m_uncommittedEventCount = -1;
                stm.m_receivedEventCount = 0;

                return Receiving;
            }

            public override State MoveToReceivingCompletedState(EventStreamConsumerStateMachine stm, int eventsCount)
            {
                throw new InvalidOperationException("Consumer stream is in receiving state.");
            }

            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumer stream is empty.");
            }

            public override State MoveToConsumedState(EventStreamConsumerStateMachine stm)
            {
                throw new InvalidOperationException("Consumer stream is empty.");
            }

            public override State MoveToClosedState(EventStreamConsumerStateMachine stm)
            {
                stm.m_uncommittedEventCount = 0;
                return Closed;
            }
        }

        private class ReceivingStartedState : State
        {
            public override State MoveToReceivingStartedState(EventStreamConsumerStateMachine stm)
            {
                throw new InvalidOperationException("Consumer stream is in receiving started state.");
            }

            public override State MoveToReceivingCompletedState(EventStreamConsumerStateMachine stm, int eventsCount)
            {
                Require.ZeroOrGreater(eventsCount, "eventsCount");

                stm.m_receivedEventCount = eventsCount;

                return Received;
            }

            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumer stream is in receiving started state.");
            }

            public override State MoveToConsumedState(EventStreamConsumerStateMachine stm)
            {
                throw new InvalidOperationException("Consumer stream is in receiving started state.");
            }

            public override StreamVersion CalculateConsumedStreamVersion(EventStreamConsumerStateMachine stm, bool skipCurrentEvent)
            {
                return stm.m_commitedVersion.Increment(stm.m_uncommittedEventCount);
            }
        }

        private class ReceivingCompletedState : State
        {
            public override State MoveToReceivingStartedState(EventStreamConsumerStateMachine stm)
            {
                if (stm.m_uncommittedEventCount == -1)
                {
                    stm.m_uncommittedEventCount = 0;
                }

                stm.m_uncommittedEventCount += stm.m_receivedEventCount;
                stm.m_receivedEventCount = 0;

                return Receiving;
            }

            public override State MoveToReceivingCompletedState(EventStreamConsumerStateMachine stm, int eventsCount)
            {
                throw new InvalidOperationException("Consumed is in events received state.");
            }

            public override State MoveToConsumingStarted()
            {
                return Consuming;
            }

            public override State MoveToConsumedState(EventStreamConsumerStateMachine stm)
            {
                throw new InvalidOperationException("Consumed is in events received state.");
            }

            public override State MoveToClosedState(EventStreamConsumerStateMachine stm)
            {
                if (stm.m_uncommittedEventCount == -1)
                {
                    stm.m_uncommittedEventCount = 0;
                }

                stm.m_uncommittedEventCount += stm.m_receivedEventCount;

                return Closed;
            }

            public override StreamVersion CalculateConsumedStreamVersion(EventStreamConsumerStateMachine stm, bool skipCurrentEvent)
            {
                if (stm.m_uncommittedEventCount == -1)
                {
                    stm.m_uncommittedEventCount = 0;
                }

                return stm.m_commitedVersion.Increment(stm.m_uncommittedEventCount + stm.m_receivedEventCount);
            }
        }

        private class ConsumingState : State
        {
            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumed is in consuming state.");
            }

            public override State MoveToConsumedState(EventStreamConsumerStateMachine stm)
            {
                stm.m_uncommittedEventCount++;

                return Consumed;
            }

            public override State MoveToClosedState(EventStreamConsumerStateMachine stm)
            {
                if (stm.m_uncommittedEventCount == -1)
                {
                    stm.m_uncommittedEventCount = 0;
                }

                return Closed;
            }

            public override void EventProcessingStarted(EventStreamConsumerStateMachine stm)
            {
                stm.m_uncommittedEventCount++;
            }

            public override StreamVersion CalculateConsumedStreamVersion(EventStreamConsumerStateMachine stm, bool skipCurrentEvent)
            {
                return stm.m_commitedVersion.Increment(
                    skipCurrentEvent
                        ? stm.m_uncommittedEventCount
                        : stm.m_uncommittedEventCount + 1);
            }
        }

        private class ConsumedState : State
        {
            public override State MoveToReceivingStartedState(EventStreamConsumerStateMachine stm)
            {
                return Receiving;
            }

            public override State MoveToReceivingCompletedState(EventStreamConsumerStateMachine stm, int eventsCount)
            {
                throw new InvalidOperationException("Consumed is in consumed state.");
            }

            public override State MoveToClosedState(EventStreamConsumerStateMachine stm)
            {
                return Closed;
            }

            public override StreamVersion CalculateConsumedStreamVersion(EventStreamConsumerStateMachine stm, bool skipCurrentEvent)
            {
                return stm.m_commitedVersion.Increment(stm.m_uncommittedEventCount);
            }
        }

        private class ClosedState : State
        {
            public override State MoveToReceivingCompletedState(EventStreamConsumerStateMachine stm, int eventsCount)
            {
                throw new InvalidOperationException("Consumed is in closed state.");
            }

            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumed is in closed state.");
            }

            public override State MoveToClosedState(EventStreamConsumerStateMachine stm)
            {
                throw new InvalidOperationException("Consumed is in closed state.");
            }

            public override StreamVersion CalculateConsumedStreamVersion(EventStreamConsumerStateMachine stm, bool skipCurrentEvent)
            {
                return stm.m_commitedVersion.Increment(stm.m_uncommittedEventCount);
            }
        }

        private StreamVersion m_commitedVersion;
        private State m_state;
        private int m_uncommittedEventCount = -1;
        private int m_receivedEventCount;

        public EventStreamConsumerStateMachine(StreamVersion commitedVersion)
        {
            m_commitedVersion = commitedVersion;
            m_state = State.Initial;
        }

        public void ReceivingStarted()
        {
            m_state = m_state.MoveToReceivingStartedState(this);
        }

        public void ReceivingCompleted(int eventsCount)
        {
            m_state = m_state.MoveToReceivingCompletedState(this, eventsCount);
        }

        public void ConsumingCompleted()
        {
            m_state = m_state.MoveToConsumedState(this);
        }

        public void ConsumingStarted()
        {
            m_state = m_state.MoveToConsumingStarted();
        }

        public void ConsumerClosed()
        {
            m_state = m_state.MoveToClosedState(this);
        }

        public void EventProcessingStarted()
        {
            m_state.EventProcessingStarted(this);
        }

        public bool CommitRequired(bool autoCommitProcessedStreamVersion)
        {
            return autoCommitProcessedStreamVersion &&
                   (m_state is ConsumedState ||  m_state is ReceivingStartedState || m_state is ClosedState) &&
                   m_uncommittedEventCount > 0;
        }

        public StreamVersion CalculateConsumedStreamVersion(bool skipCurrentEvent)
        {
            return m_state.CalculateConsumedStreamVersion(this, skipCurrentEvent);
        }

        public void ConsumedStreamVersionCommited(StreamVersion version, bool skipCurrent)
        {
            m_state.ConsumedStreamVersionCommited(this, version, skipCurrent);
        }

        public StreamVersion CommitedStreamVersion
        {
            get { return m_commitedVersion; }
        }
    }
}
