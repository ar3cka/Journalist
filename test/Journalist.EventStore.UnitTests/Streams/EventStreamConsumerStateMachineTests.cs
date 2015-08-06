using System;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamConsumerStateMachineTests
    {
        [Theory, AutoMoqData]
        public void ConsumerEventsReceived_WhenStmInInitalState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();
        }

        [Theory, AutoMoqData]
        public void ConsumerEventsReceived_WhenStmInClosedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumerEventsReceived());
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInInitialState_Throws(EventStreamConsumerStateMachine stm)
        {
            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInEventsReceivedState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();
            stm.ConsumingStarted();
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInClosedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingStarted_WhenStmInConsumingState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();
            stm.ConsumingStarted();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingStarted());
        }

        [Theory, AutoMoqData]
        public void ConsumingCompleted_WhenStmInInitialState_Throws(EventStreamConsumerStateMachine stm)
        {
            Assert.Throws<InvalidOperationException>(() => stm.ConsumingCompleted());
        }

        [Theory, AutoMoqData]
        public void ConsumingCompleted_WhenStmInEventsReceivedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumingCompleted());
        }

        [Theory, AutoMoqData]
        public void ConsumingCompleted_WhenStmInConsumingState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();
            stm.ConsumingStarted();
            stm.ConsumingCompleted();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmInInitialState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmInEventReceivedState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();
            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmConsumingState_DoesNotThrow(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerEventsReceived();
            stm.ConsumingStarted();
            stm.ConsumerClosed();
        }

        [Theory, AutoMoqData]
        public void ConsumerClosed_WhenStmInClosedState_Throws(EventStreamConsumerStateMachine stm)
        {
            stm.ConsumerClosed();

            Assert.Throws<InvalidOperationException>(() => stm.ConsumerClosed());
        }
    }
}
