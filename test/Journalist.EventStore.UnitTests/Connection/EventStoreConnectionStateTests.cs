using System;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Connection
{
    public class EventStoreConnectionStateTests
    {
        [Theory, AutoMoqData]
        public void NewState_PropertiesTests(
            EventStoreConnectionState state)
        {
            Assert.False(state.IsActive);
            Assert.False(state.IsClosing);
            Assert.False(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_FiresWhenObejectIsInClosedState_Throws(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated());
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_WhenObejectIsInClosedState_Throws(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated());
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_WhenObejectIsInClosingState_Throws(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated());
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_WhenObejectIsInActiveState_Throws(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated());
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_PropertiesTests(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();

            Assert.True(state.IsActive);
            Assert.False(state.IsClosed);
            Assert.False(state.IsClosing);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosing_PropertiesTests(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();

            Assert.False(state.IsActive);
            Assert.True(state.IsClosing);
            Assert.False(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_WhenObejectIsNotInClosingState_Throws(
            EventStoreConnectionState state)
        {
            Assert.Throws<InvalidOperationException>(() => state.ChangeToClosed());
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_PropertiesTests(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.False(state.IsActive);
            Assert.False(state.IsClosing);
            Assert.True(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void EnsureConnectionIsActive_WhenObjectIsInClosingState_Throws(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();

            Assert.Throws<EventStreamConnectionWasClosedException>(
                () => state.EnsureConnectionIsActive());
        }

        [Theory, AutoMoqData]
        public void EnsureConnectionIsActive_WhenObjectIsInClosedState_Throws(
            EventStoreConnectionState state)
        {
            state.ChangeToCreated();
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<EventStreamConnectionWasClosedException>(
                () => state.EnsureConnectionIsActive());
        }
    }
}
