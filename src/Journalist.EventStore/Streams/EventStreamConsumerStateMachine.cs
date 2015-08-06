using System;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumerStateMachine
    {
        private abstract class State
        {
            public virtual State MoveToEventsReceivedState()
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToConsumingStarted()
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToConsumedState()
            {
                throw new NotImplementedException();
            }

            public virtual State MoveToClosedState()
            {
                throw new NotImplementedException();
            }
        }

        private class InitialState : State
        {
            public override State MoveToEventsReceivedState()
            {
                return States.EventsReceived;
            }

            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumer stream is empty.");
            }

            public override State MoveToConsumedState()
            {
                throw new InvalidOperationException("Consumer stream is empty.");
            }

            public override State MoveToClosedState()
            {
                return States.Closed;
            }
        }

        private class EventsReceivedState : State
        {
            public override State MoveToConsumingStarted()
            {
                return States.ConsumingState;
            }

            public override State MoveToConsumedState()
            {
                throw new InvalidOperationException("Consumed is in events received state.");
            }

            public override State MoveToClosedState()
            {
                return States.Closed;
            }
        }

        private class ConsumingState : State
        {
            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumed is in consuming state.");
            }

            public override State MoveToConsumedState()
            {
                return States.ConsumedState;
            }

            public override State MoveToClosedState()
            {
                return States.Closed;
            }
        }

        private class ConsumedState : State
        {
        }

        private class ClosedState : State
        {
            public override State MoveToEventsReceivedState()
            {
                throw new InvalidOperationException("Consumed is in closed state.");
            }

            public override State MoveToConsumingStarted()
            {
                throw new InvalidOperationException("Consumed is in closed state.");
            }

            public override State MoveToClosedState()
            {
                throw new InvalidOperationException("Consumed is in closed state.");
            }
        }

        private static class States
        {
            public static readonly State Initial = new InitialState();
            public static readonly State EventsReceived = new EventsReceivedState();
            public static readonly State ConsumingState = new ConsumingState();
            public static readonly State ConsumedState = new ConsumedState();
            public static readonly State Closed = new ClosedState();
        }

        private StreamVersion m_commitedVersion;
        private State m_state;

        public EventStreamConsumerStateMachine(StreamVersion commitedVersion)
        {
            m_commitedVersion = commitedVersion;
            m_state = States.Initial;
        }

        public void ConsumingStarted()
        {
            m_state = m_state.MoveToConsumingStarted();
        }

        public void ConsumerClosed()
        {
            m_state = m_state.MoveToClosedState();
        }

        public void ConsumerEventsReceived()
        {
            m_state = m_state.MoveToEventsReceivedState();
        }

        public void ConsumingCompleted()
        {
            m_state = m_state.MoveToConsumedState();
        }
    }
}
