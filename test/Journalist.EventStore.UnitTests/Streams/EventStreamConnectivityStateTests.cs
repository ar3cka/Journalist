using System;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamConnectivityStateTests
    {
        [Theory, AutoMoqData]
        public void NewState_PropertiesTests(
            EventStreamConnectivityState state)
        {
            Assert.True(state.IsActive);
            Assert.False(state.IsClosing);
            Assert.False(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosingWhenObejectIsInClosedState_Throws(
            EventStreamConnectivityState state)
        {
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToClosed());
        }

        [Theory, AutoMoqData]
        public void ChangeToClosing_PropertiesTests(
            EventStreamConnectivityState state)
        {
            state.ChangeToClosing();

            Assert.False(state.IsActive);
            Assert.True(state.IsClosing);
            Assert.False(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_WhenObejectIsNotInClosingState_Throws(
            EventStreamConnectivityState state)
        {
            Assert.Throws<InvalidOperationException>(() => state.ChangeToClosed());
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_PropertiesTests(
            EventStreamConnectivityState state)
        {
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.False(state.IsActive);
            Assert.False(state.IsClosing);
            Assert.True(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void EnsureConnectionIsActive_WhenObjectIsInClosingState_Throws(
            EventStreamConnectivityState state)
        {
            state.ChangeToClosing();

            Assert.Throws<EventStreamConnectionWasClosedException>(
                () => state.EnsureConnectionIsActive());
        }

        [Theory, AutoMoqData]
        public void EnsureConnectionIsActive_WhenObjectIsInClosedState_Throws(
            EventStreamConnectivityState state)
        {
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<EventStreamConnectionWasClosedException>(
                () => state.EnsureConnectionIsActive());
        }
    }
}
