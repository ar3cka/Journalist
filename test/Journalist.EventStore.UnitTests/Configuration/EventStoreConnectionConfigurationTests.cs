using Journalist.EventStore.Configuration;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Configuration
{
    public class EventStoreConnectionConfigurationTests
    {
        [Theory, AutoMoqData]
        public void IncomingEventsWith_AddsItToIncomingMutatorsList(
            IEventMutator mutator,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            connectionConfiguration.Mutate.IncomingEventsWith(mutator);

            Assert.Contains(mutator, connectionConfiguration.IncomingMessageMutators);
        }

        [Theory, AutoMoqData]
        public void OutgoingEventsWith_AddsItToOutgoingMutatorsList(
            IEventMutator mutator,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            connectionConfiguration.Mutate.OutgoingEventsWith(mutator);

            Assert.Contains(mutator, connectionConfiguration.OutgoingMessageMutators);
        }

        [Theory, AutoMoqData]
        public void IncomingEventsWith_ReturnsInstanceOfConnectionConfiguration(
            IEventMutator mutator,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            Assert.Same(connectionConfiguration, connectionConfiguration.Mutate.IncomingEventsWith(mutator));
        }

        [Theory, AutoMoqData]
        public void OutgoingEventsWith_ReturnsInstanceOfConnectionConfiguration(
            IEventMutator mutator,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            Assert.Same(connectionConfiguration, connectionConfiguration.Mutate.OutgoingEventsWith(mutator));
        }

        [Theory, AutoMoqData]
        public void NotificationsSubscribe_ReturnsInstanceOfConnectionConfiguration(
            INotificationListener listener,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            Assert.Same(connectionConfiguration, connectionConfiguration.Notifications.Subscribe(listener));
        }

        [Theory, AutoMoqData]
        public void NotificationsSubscribe_WhenProcessingIsEnabled_AddsListenerToCollection(
            INotificationListener listener,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            connectionConfiguration.Notifications.EnableProcessing();

            connectionConfiguration.Notifications.Subscribe(listener);

            Assert.Contains(listener, connectionConfiguration.NotificationListeners);
        }

        [Theory, AutoMoqData]
        public void NotificationsSubscribe_WhenProcessingIsNotEnabled_AddsListenerToCollection(
            INotificationListener listener,
            EventStoreConnectionConfiguration connectionConfiguration)
        {
            connectionConfiguration.Notifications.Subscribe(listener);

            Assert.DoesNotContain(listener, connectionConfiguration.NotificationListeners);
        }
    }
}
