using System;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamConsumerStateMachineTests
    {
        [Theory, AutoMoqData]
        public void ReceivingStarted_WhenStmInInitalState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ReceivingStarted();
        }

        [Theory, AutoMoqData]
        public void ReceivingStarted_WhenStmInReceivingStatedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ReceivingStarted();

            Assert.Throws<InvalidOperationException>(() => stm.ReceivingStarted());
        }

        [Theory, AutoMoqData]
        public void ReceivingStarted_WhenStmInReceivingCompletedState_DoesNotThrow(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);

            stm.ReceivingStarted();
        }

        [Theory, AutoMoqData]
        public void ReceivingCompleted_WhenStmInInitalState_Throws(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Assert.Throws<InvalidOperationException>(() => stm.ReceivingCompleted(eventCount));
        }

        [Theory, AutoMoqData]
        public void ReceivingCompleted_WhenStmInReceivingStartedState_DoesNotThrows(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);
        }

        [Theory, AutoMoqData]
        public void ReceivingCompleted_WhenStmInEventsReceivedState_Throws(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);

            Assert.Throws<InvalidOperationException>(() => stm.ReceivingCompleted(eventCount));
        }

        [Theory, AutoMoqData]
        public void ReceivingCompleted_WhenStmInConsumedState_Throws(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);
            Consume(stm);

            Assert.Throws<InvalidOperationException>(() => stm.ReceivingCompleted(eventCount));
        }

        [Theory, AutoMoqData]
        public void ReceivingCompleted_WhenStmInClosedState_Throws(EventStreamConsumerStateMachine stm, int eventCount)
        {
            stm.ConsumerClosed();

            Assert.Throws<InvalidOperationException>(() => stm.ReceivingCompleted(eventCount));
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInInitialState_Throws(EventStreamConsumerStateMachine stm)
        {
            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInReceivingStartedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ReceivingStarted();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInReceivingCompletedState_DoesNotThrow(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);

            stm.ConsumingStarted();
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInClosedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInConsumingState_Throws(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);
            stm.ConsumingStarted();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingCompleted_WhenStmInInitialState_Throws(EventStreamConsumerStateMachine stm)
        {
            Assert.Throws<InvalidOperationException>(() => stm.ConsumingCompleted());
        }

        [Theory, AutoMoqData]
        public void ConsumingCompleted_WhenStmInEventsReceivedState_Throws(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingCompleted());
        }

        [Theory, AutoMoqData]
        public void ConsumingCompleted_WhenStmInConsumingState_DoesNotThrow(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);
            Consume(stm);
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmInInitialState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmInEventReceivedState_DoesNotThrow(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);

            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmConsumingState_DoesNotThrow(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);

            stm.ConsumingStarted();
            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmConsumedState_DoesNotThrow(EventStreamConsumerStateMachine stm, int eventCount)
        {
            Receive(stm, eventCount);
            Consume(stm);

            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmInClosedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumerClosed());
        }

        private static void Receive(EventStreamConsumerStateMachine stm, int eventCount)
        {
            stm.ReceivingStarted();
            stm.ReceivingCompleted(eventCount);
        }

        private static void Consume(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumingStarted();
            stm.ConsumingCompleted();
        }
    }
}
