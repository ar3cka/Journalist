using System;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Listeners
{
    public class NotificationListenerSubscriptionTest
    {
        [Theory, NotificationListenerSubscriptionData]
        public void Start_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription)
        {
            subscription.Start(connection);

            listenerMock.Verify(self => self.OnSubscriptionStarted(subscription), Times.Once());
        }

        [Theory, NotificationListenerSubscriptionData]
        public void Stop_WhenSubscriptionHasNotBeenStarted_Throws(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription,
            INotification notification)
        {
            Assert.Throws<InvalidOperationException>(() => subscription.Stop());
        }

        [Theory, NotificationListenerSubscriptionData]
        public void Stop_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            INotification notification)
        {
            subscription.Start(connection);
            subscription.Stop();

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }

        [Theory, NotificationListenerSubscriptionData]
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

        [Theory, NotificationListenerSubscriptionData]
        public async Task HandleNotificationAsync_WhenNotificationNotAddressedToConsumer_PropagatesNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            subscription.Start(connection);

            await subscription.HandleNotificationAsync(notification.SendTo(listenerMock.Object));

            listenerMock.Verify(
                self => self.On(notification),
                Times.Never());
        }

        [Theory, NotificationListenerSubscriptionData]
        public async Task HandleNotificationAsync_WhenNotificationAddressedToConsumer_PropagatesNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] EventStreamReaderId consumerId,
            IEventStoreConnection connection,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            var addressedNotification = (EventStreamUpdated)notification.SendTo(listenerMock.Object);

            subscription.Start(connection);

            await subscription.HandleNotificationAsync(addressedNotification);

            listenerMock.Verify(
                self => self.On(addressedNotification),
                Times.Once());
        }

        [Theory, NotificationListenerSubscriptionData]
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

        [Theory, NotificationListenerSubscriptionData]
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

        [Theory, NotificationListenerSubscriptionData]
        public void CreateSubscriptionConsumerAsync_WhenSubscriptionWasStarted_UseConnection(
            [Frozen] EventStreamReaderId consumerId,
            Mock<IEventStoreConnection> connectionMock,
            NotificationListenerSubscription subscription,
            string streamName)
        {
            subscription.Start(connectionMock.Object);

            subscription.CreateSubscriptionConsumerAsync(streamName, true);

            connectionMock.Verify(self => self.CreateStreamConsumerAsync(
                It.IsAny<Action<IEventStreamConsumerConfiguration>>()));
        }

        [Theory, NotificationListenerSubscriptionData]
        public async Task CreateSubscriptionConsumerAsync_WhenSubscriptionWasNotStarted_Throws(
            [Frozen] EventStreamReaderId consumerId,
            NotificationListenerSubscription subscription,
            string streamName)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => subscription.CreateSubscriptionConsumerAsync(streamName, true));
        }

        [Theory, NotificationListenerSubscriptionData]
        public async Task DefferNotificationAsync_SendAddressedNotification(
            [Frozen] Mock<INotificationsChannel> channelMock,
            [Frozen] Mock<INotification> receivedNotificationMock,
            INotification deferredNotification,
            IEventStoreConnection connection,
            INotificationListener listener,
            NotificationListenerSubscription subscription)
        {
            receivedNotificationMock
                .Setup(self => self.SendTo(listener))
                .Returns(deferredNotification);

            subscription.Start(connection);

            await subscription.RetryNotificationProcessingAsync(receivedNotificationMock.Object);

            channelMock
                .Verify(self => self.SendAsync(
                    deferredNotification,
                    It.Is<TimeSpan>(v => TimeSpan.FromSeconds(deferredNotification.DeliveryCount * 2) == v)));
        }
    }
}
