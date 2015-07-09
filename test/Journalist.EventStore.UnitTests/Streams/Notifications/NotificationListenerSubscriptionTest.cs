using Journalist.EventStore.Streams.Notifications;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams.Notifications
{
    public class NotificationListenerSubscriptionTest
    {
        [Theory, AutoMoqData]
        public void SubscriptionStart_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Start();

            listenerMock.Verify(self => self.OnSubscriptionStarted(), Times.Once());
        }

        [Theory, AutoMoqData]
        public void SubscriptionStop_NotifyListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationListenerSubscription subscription,
            EventStreamUpdated notification)
        {
            subscription.Stop();

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }
    }
}
