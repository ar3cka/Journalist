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
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated(connection));
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_FiresConnectionCreatedEvent(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            var fired = false;
            state.ConnectionCreated += (sender, args) => fired = true;

            state.ChangeToCreated(connection);

            Assert.True(fired);
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_WhenObejectIsInClosedState_Throws(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated(connection));
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_WhenObejectIsInClosingState_Throws(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated(connection));
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_WhenObejectIsInActiveState_Throws(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);

            Assert.Throws<InvalidOperationException>(() => state.ChangeToCreated(connection));
        }

        [Theory, AutoMoqData]
        public void ChangeToCreated_PropertiesTests(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);

            Assert.True(state.IsActive);
            Assert.False(state.IsClosed);
            Assert.False(state.IsClosing);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosing_PropertiesTests(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();

            Assert.False(state.IsActive);
            Assert.True(state.IsClosing);
            Assert.False(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosing_FiresConnectionClosingEvent(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            var fired = false;
            state.ConnectionClosing += (sender, args) => fired = true;

            state.ChangeToCreated(connection);
            state.ChangeToClosing();

            Assert.True(fired);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_WhenObejectIsNotInClosingState_Throws(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            Assert.Throws<InvalidOperationException>(() => state.ChangeToClosed());
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_PropertiesTests(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.False(state.IsActive);
            Assert.False(state.IsClosing);
            Assert.True(state.IsClosed);
        }

        [Theory, AutoMoqData]
        public void ChangeToClosed_FiresConnectionClosedEvent(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            var fired = false;
            state.ConnectionClosed += (sender, args) => fired = true;

            state.ChangeToCreated(connection);
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.True(fired);
        }

        [Theory, AutoMoqData]
        public void EnsureConnectionIsActive_WhenObjectIsInClosingState_Throws(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();

            Assert.Throws<EventStreamConnectionWasClosedException>(
                () => state.EnsureConnectionIsActive());
        }

        [Theory, AutoMoqData]
        public void EnsureConnectionIsActive_WhenObjectIsInClosedState_Throws(
            IEventStoreConnection connection,
            EventStoreConnectionState state)
        {
            state.ChangeToCreated(connection);
            state.ChangeToClosing();
            state.ChangeToClosed();

            Assert.Throws<EventStreamConnectionWasClosedException>(
                () => state.EnsureConnectionIsActive());
        }
    }
}
