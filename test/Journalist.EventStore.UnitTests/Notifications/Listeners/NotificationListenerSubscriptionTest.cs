using System;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Listeners
{
    public class NotificationListenerSubscriptionTest
    {
        [Theory]
        [AutoMoqData]
        public void Start_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription)
        {
            subscription.Start(connection);

            listenerMock.Verify(self => self.OnSubscriptionStarted(subscription), Times.Once());
        }

        [Theory, AutoMoqData]
        public void Stop_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Stop();

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }

        [Theory, AutoMoqData]
        public async Task HandleNotificationAsync_WhenSubscriptionStarted_PropagatesNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Start(connection);

            await subscription.HandleNotificationAsync(notification);

            listenerMock.Verify(
                self => self.On(notification),
                Times.Once());
        }

        [Theory, AutoMoqData]
        public async Task HandleNotificationAsync_WhenNotificationNotAddressedToConsumer_PropagatesNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            subscription.Start(connection);

            await subscription.HandleNotificationAsync(notification.SendTo(consumerId));

            listenerMock.Verify(
                self => self.On(notification),
                Times.Never());
        }

        [Theory, AutoMoqData]
        public async Task HandleNotificationAsync_WhenNotificationAddressedToConsumer_PropagatesNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] EventStreamConsumerId consumerId,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            var addressedNotification = (EventStreamUpdated)notification.SendTo(consumerId);

            subscription.Start(connection);

            await subscription.HandleNotificationAsync(addressedNotification);

            listenerMock.Verify(
                self => self.On(addressedNotification),
                Times.Once());
        }

        [Theory, AutoMoqData]
        public async Task HandleNotificationAsync_WhenSubscriptionDidNotStart_NotPropagatesNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            await subscription.HandleNotificationAsync(notification);

            listenerMock.Verify(
                self => self.On(notification),
                Times.Never());
        }

        [Theory, AutoMoqData]
        public async Task HandleNotificationAsync__WhenSubscriptionStoped_PropagateNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Start(connection);
            await subscription.HandleNotificationAsync(notification);

            subscription.Stop();
            await subscription.HandleNotificationAsync(notification);

            listenerMock.Verify(
                self => self.On(notification),
                Times.Once());
        }

        [Theory, AutoMoqData]
        public void CreateSubscriptionConsumerAsync_WhenSubscriptionWasStarted_UseConnection(
            [Frozen] EventStreamConsumerId consumerId,
            Mock<IEventStoreConnection> connectionMock,
            NotificationListenerSubscription subscription,
            string streamName)
        {
            subscription.Start(connectionMock.Object);

            subscription.CreateSubscriptionConsumerAsync(streamName);

            connectionMock.Verify(self => self.CreateStreamConsumerAsync(
                It.IsAny<Action<IEventStreamConsumerConfiguration>>()));
        }

        [Theory, AutoMoqData]
        public async Task CreateSubscriptionConsumerAsync_WhenSubscriptionWasNotStarted_Throws(
            [Frozen] EventStreamConsumerId consumerId,
            NotificationListenerSubscription subscription,
            string streamName)
        {
            await
                Assert.ThrowsAsync<InvalidOperationException>(
                    () => subscription.CreateSubscriptionConsumerAsync(streamName));
        }

        [Theory, AutoMoqData]
        public async Task DefferNotificationAsync_SendAddressedNotification(
            [Frozen] Mock<INotificationHub> hubMock,
            [Frozen] EventStreamConsumerId consumerId,
            Mock<INotification> receivedNotificationMock,
            INotification deferredNotification,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription)
        {
            receivedNotificationMock
                .Setup(self => self.SendTo(consumerId))
                .Returns(deferredNotification);

            subscription.Start(connection);

            await subscription.DefferNotificationAsync(receivedNotificationMock.Object);

            hubMock
                .Verify(self => self.NotifyAsync(deferredNotification));
        }
    }
}
