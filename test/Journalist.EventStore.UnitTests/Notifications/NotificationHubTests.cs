using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.EventStore.Utils.Polling;
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
            EventStreamUpdated notification)
        {
            await hub.NotifyAsync(notification);

            channelMock.Verify(self => self.SendAsync(notification), Times.Once());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_NotifiesListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub, connection);

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }

        [Theory, NotificationHubData(emptyChannel: true)]
        public async Task StopNotificationProcessing_WhenChannelIsEmpty_WaitsTimeout(
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            IEventStoreConnection connection,
            INotificationListener listener,
            NotificationHub hub)
        {
            hub.Subscribe(listener);

            await RunNotificationProcessingTest(hub, connection);

            timeoutMock.Verify(self => self.WaitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, NotificationHubData(emptyChannel: true)]
        public async Task StopNotificationProcessing_WhenChannelIsEmpty_IncreaseTimeout(
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            IEventStoreConnection connection,
            INotificationListener listener,
            NotificationHub hub)
        {
            hub.Subscribe(listener);

            await RunNotificationProcessingTest(hub, connection);

            timeoutMock.Verify(self => self.Increase(), Times.Once());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_WhenChannelIsNotEmpty_DoesNotWaitTimeout(
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            IEventStoreConnection connection,
            INotificationListener listener,
            NotificationHub hub)
        {
            hub.Subscribe(listener);

            await RunNotificationProcessingTest(hub, connection);

            timeoutMock.Verify(self => self.WaitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_WhenChannelIsNotEmpty_ResetsTimout(
            [Frozen] Mock<IPollingTimeout> timeoutMock,
            IEventStoreConnection connection,
            INotificationListener listener,
            NotificationHub hub)
        {
            hub.Subscribe(listener);

            await RunNotificationProcessingTest(hub, connection);

            timeoutMock.Verify(self => self.Reset(), Times.AtLeastOnce());
        }

        [Theory, NotificationHubData]
        public async Task StopNotificationProcessing_WhenChannelIsNotEmpty_PropagatesNotificationsToListeners(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.Subscribe(listenerMock.Object);

            await RunNotificationProcessingTest(hub, connection);

            listenerMock.Verify(
                self => self.On(It.IsAny<EventStreamUpdated>()),
                Times.AtLeastOnce());
        }

        [Theory, NotificationHubData]
        public void StartNotificationProcessing_WhenListenerListIsEmpty_NeverCallsChannelReceiveNotificationsAsync(
            [Frozen] Mock<INotificationsChannel> channelMock,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.StartNotificationProcessing(connection);

            channelMock.Verify(
                self => self.ReceiveNotificationsAsync(),
                Times.Never());
        }

        [Theory, NotificationHubData]
        public void StartNotificationProcessing_WhenListenerWasRemoved_NeverCallsChannelReceiveNotificationsAsync(
            [Frozen] Mock<INotificationsChannel> channelMock,
            IEventStoreConnection connection,
            INotificationListener listener,
            NotificationHub hub)
        {
            hub.Subscribe(listener);
            hub.Unsubscribe(listener);

            hub.StartNotificationProcessing(connection);

            channelMock.Verify(
                self => self.ReceiveNotificationsAsync(),
                Times.Never());
        }

        private static async Task RunNotificationProcessingTest(NotificationHub hub, IEventStoreConnection connection)
        {
            hub.StartNotificationProcessing(connection);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            hub.StopNotificationProcessing();
        }
    }
}
