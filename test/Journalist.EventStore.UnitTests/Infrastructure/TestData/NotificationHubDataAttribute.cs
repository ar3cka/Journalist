using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Timeouts;
using Journalist.EventStore.Notifications.Types;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class NotificationHubDataAttribute : AutoMoqDataAttribute
    {
        public NotificationHubDataAttribute(bool emptyChannel = false)
        {
            Fixture.Customize<INotification[]>(composer => composer
                .FromFactory((EventStreamUpdated[] notifications) => notifications));

            Fixture.Customize<Mock<INotificationsChannel>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.ReceiveNotificationsAsync())
                    .Returns(() => emptyChannel
                        ? EmptyArray.Get<INotification>().YieldTask()
                        : Fixture.Create<INotification[]>().YieldTask()))
                .Do(mock => mock
                    .Setup(self => self.SendAsync(It.IsAny<INotification>()))
                    .Returns(TaskDone.Done)));

            Fixture.Customize<Mock<IPollingTimeout>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.WaitAsync(It.IsAny<CancellationToken>()))
                    .Returns((CancellationToken token) => Task.WhenAny(Task.Delay(TimeSpan.FromMinutes(1), token)))));

            Fixture.Customize<Mock<INotificationFormatter>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.FromBytes(It.IsAny<Stream>()))
                    .Returns(() => Fixture.Create<EventStreamUpdated>()))
                .Do(mock => mock
                    .Setup(self => self.ToBytes(It.IsAny<EventStreamUpdated>()))
                    .ReturnsUsingFixture(Fixture)));
        }
    }
}
