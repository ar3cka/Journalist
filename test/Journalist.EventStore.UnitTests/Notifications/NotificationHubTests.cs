using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Timeouts;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications
{
    public class NotificationHubTests
    {
        [Theory, NotificationHubData]
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

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_NotifiesListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub);

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }

        [Theory, NotificationHubData(emptyChannel: true)]
        public async Task StopNotificationProcessing_WhenChannelIsEmpty_WaitsTimeout(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub);

            timeoutMock.Verify(self => self.WaitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, NotificationHubData(emptyChannel: true)]
        public async Task StopNotificationProcessing_WhenChannelIsEmpty_IncreaseTimeout(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub);

            timeoutMock.Verify(self => self.Increase(), Times.Once());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_WhenChannelIsNotEmpty_DoesNotWaitTimeout(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub);

            timeoutMock.Verify(self => self.WaitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_WhenChannelIsNotEmpty_ResetsTimout(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub);

            timeoutMock.Verify(self => self.Reset(), Times.AtLeastOnce());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_WhenChannelIsNotEmpty_PropagatesNotificationsToListeners(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            NotificationHub hub,
            EventStreamUpdated notification,
            Stream notificationBytes)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub);

            listenerMock.Verify(
                self => self.OnAsync(It.IsAny<EventStreamUpdated>()),
                Times.AtLeastOnce());
        }

        private static async Task RunNotificationProcessingTest(NotificationHub hub)
        {
            hub.StartNotificationProcessing();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            hub.StopNotificationProcessing();
        }
    }
}
