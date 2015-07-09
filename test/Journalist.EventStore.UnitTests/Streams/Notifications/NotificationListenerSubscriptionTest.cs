using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Streams.Notifications;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams.Notifications
{
    public class NotificationListenerSubscriptionTest
    {
        [Theory]
        [AutoMoqData]
        public void Start_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription)
        {
            subscription.Start();

            listenerMock.Verify(self => self.OnSubscriptionStarted(), Times.Once());
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
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Start();

            await subscription.HandleNotificationAsync(notification);

            listenerMock.Verify(
                self => self.OnAsync(notification),
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
                self => self.OnAsync(notification),
                Times.Never());
        }

        [Theory, AutoMoqData]
        public async Task HandleNotificationAsync__WhenSubscriptionStoped_PropagateNotificationToTheListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Start();
            await subscription.HandleNotificationAsync(notification);

            subscription.Stop();
            await subscription.HandleNotificationAsync(notification);

            listenerMock.Verify(
                self => self.OnAsync(notification),
                Times.Once());
        }
    }
}
