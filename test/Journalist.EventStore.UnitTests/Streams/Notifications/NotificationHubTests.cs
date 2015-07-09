using System.IO;
using System.Threading.Tasks;
using Journalist.EventStore.Streams.Notifications;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams.Notifications
{
    public class NotificationHubTests
    {
        [Theory, AutoMoqData]
        public async Task NotifyAsync_SendsNotificationBytesToChannel(
            [Frozen] Mock<INotificationFormatter> formatterMock,
            [Frozen] Mock<INotificationsChannel> channelMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            formatterMock
                .Setup(self => self.ToBytes(notification))
                .Returns(notificationBytes);

            await hub.NotifyAsync(notification);

            channelMock.Verify(self => self.SendAsync(notificationBytes), Times.Once());
        }

        [Theory, AutoMoqData]
        public void StopNotificationProcessing_NotifiesListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            hub.StartNotificationProcessing();
            hub.StopNotificationProcessing();

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }
    }
}
