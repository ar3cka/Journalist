using System.Threading.Tasks;
using Journalist.EventStore.Streams.Notifications;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams.Notifications
{
    public class NotificationHubTests
    {
        [Theory, AutoMoqData]
        public async Task NotifyAsync_WhenSubscriptionHasNotStarted_NotPropagateNotificationToTheListener(
            NotificationHub hub,
            Mock<INotificationListener> listenerMock,
            EventStreamUpdated notification)
        {
            hub.Subscribe(listenerMock.Object);

            await hub.NotifyAsync(notification);

            listenerMock.Verify(
                self => self.OnEventStreamUpdatedAsync(notification),
                Times.Never());
        }

        [Theory, AutoMoqData]
        public async Task NotifyAsync_WhenSubscriptionHasStarted_PropagateNotificationToTheListener(
            NotificationHub hub,
            Mock<INotificationListener> listenerMock,
            EventStreamUpdated notification)
        {
            hub.Subscribe(listenerMock.Object).Start();

            await hub.NotifyAsync(notification);

            listenerMock.Verify(
                self => self.OnEventStreamUpdatedAsync(notification),
                Times.Once());
        }

        [Theory, AutoMoqData]
        public async Task NotifyAsync_WhenSubscriptionHasStoped_PropagateNotificationToTheListener(
            NotificationHub hub,
            Mock<INotificationListener> listenerMock,
            EventStreamUpdated notification)
        {
            var subscription = hub.Subscribe(listenerMock.Object);

            subscription.Start();
            await hub.NotifyAsync(notification);

            subscription.Stop();
            await hub.NotifyAsync(notification);

            listenerMock.Verify(
                self => self.OnEventStreamUpdatedAsync(notification),
                Times.Once());
        }
    }
}
