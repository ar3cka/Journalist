using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Listeners;
using Moq;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class NotificationListenerSubscriptionDataAttribute : AutoMoqDataAttribute
    {
        public NotificationListenerSubscriptionDataAttribute()
        {
            Fixture.Customize<Mock<INotification>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.SendTo(It.IsAny<INotificationListener>()))
                    .ReturnsUsingFixture(Fixture))
                .Do(mock => mock
                    .SetupGet(self => self.DeliveryCount)
                    .Returns(0)));
        }
    }
}
