using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.Utils.Polling;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class NotificationHubDataAttribute : AutoMoqDataAttribute
    {
        public NotificationHubDataAttribute(
            bool emptyChannel = false,
            bool hasSubscriber = true,
            bool startHub = true)
        {

            Fixture.Customize<INotification>(composer => composer
                .FromFactory((EventStreamUpdated notification) => notification));

            Fixture.Customize<IReceivedNotification[]>(composer => composer
                .FromFactory((IReceivedNotification n) => n.YieldArray()));

            Fixture.Customize<Mock<INotificationsChannel>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.ReceiveNotificationsAsync())
                    .Returns(() => emptyChannel
                        ? EmptyArray.Get<IReceivedNotification>().YieldTask()
                        : Fixture.Create<IReceivedNotification[]>().YieldTask()))
                .Do(mock => mock
                    .Setup(self => self.SendAsync(It.IsAny<INotification>()))
                    .Returns(TaskDone.Done)));

            Fixture.Customize<IPollingJob>(composer => composer.FromFactory((PollingJobStub stub) => stub));

            Fixture.Customize<Mock<INotificationFormatter>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.FromBytes(It.IsAny<Stream>()))
                    .Returns(() => Fixture.Create<EventStreamUpdated>()))
                .Do(mock => mock
                    .Setup(self => self.ToBytes(It.IsAny<EventStreamUpdated>()))
                    .ReturnsUsingFixture(Fixture)));

            Fixture.Customize<NotificationHub>(composer => composer
                .Do(hub =>
                {
                    if (hasSubscriber)
                    {
                        hub.Subscribe(Fixture.Create<INotificationListener>());
                    }

                    if (startHub)
                    {
                        hub.StartNotificationProcessing(Fixture.Create<IEventStoreConnection>());
                    }
                }));
        }
    }
}
