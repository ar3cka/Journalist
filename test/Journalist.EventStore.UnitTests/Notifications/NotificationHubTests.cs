using System;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
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
            EventStreamUpdated notification)
        {
            await hub.NotifyAsync(notification);

            channelMock.Verify(self => self.SendAsync(notification), Times.Once());
        }

        [Theory, NotificationHubData(startHub: false)]
        public void StartNotificationProcessing_NotifiesListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.StartNotificationProcessing(connection);

            listenerMock.Verify(self => self.OnSubscriptionStarted(It.IsAny<INotificationListenerSubscription>()), Times.Once());
        }

        [Theory, NotificationHubData]
        public void StopNotificationProcessing_NotifiesListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.StopNotificationProcessing();

            listenerMock.Verify(self => self.OnSubscriptionStopped(), Times.Once());
        }

        [Theory, NotificationHubData(startHub: false)]
        public void StartNotificationProcessing_StartsPollingJob(
            [Frozen] PollingJobStub jobStub,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.StartNotificationProcessing(connection);

            Assert.True(jobStub.JobStarted);
            Assert.False(jobStub.JobStoped);
        }

        [Theory, NotificationHubData]
        public void StoptNotificationProcessing_StopsPollingJob(
            [Frozen] PollingJobStub jobStub,
            NotificationHub hub)
        {
            hub.StopNotificationProcessing();

            Assert.False(jobStub.JobStarted);
            Assert.True(jobStub.JobStoped);
        }

        [Theory, NotificationHubData(startHub: false)]
        public void StoptNotificationProcessing_WhenHubWasNotStarted_Throws(
            NotificationHub hub)
        {
            Assert.Throws<InvalidOperationException>(() => hub.StopNotificationProcessing());
        }

        [Theory, NotificationHubData(emptyChannel: true)]
        public async Task PollingFunc_WhenChannelIsEmpty_ReturnsFalse(
            [Frozen] PollingJobStub jobStub,
            NotificationHub hub)
        {
            var pollResult = await jobStub.Poll();

            Assert.False(pollResult);
        }

        [Theory, NotificationHubData]
        public async Task PollingFunc_WhenChannelIsNotEmpty_ReturnsTrue(
            [Frozen] PollingJobStub jobStub,
            NotificationHub hub)
        {
            var pollResult = await jobStub.Poll();

            Assert.True(pollResult);
        }

        [Theory, NotificationHubData]
        public async Task PollingFunc_WhenChannelIsNotEmpty_SendsNotificationsToListener(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] PollingJobStub jobStub,
            NotificationHub hub)
        {
            await jobStub.Poll();

            listenerMock.Verify(self => self.On(It.IsAny<EventStreamUpdated>()), Times.AtLeastOnce());
        }

        [Theory, NotificationHubData]
        public async Task PollingFunc_WhenNotificationProcessingWasSuccessfullyCompleted_CompletesNotification(
            [Frozen] Mock<IReceivedNotification> notificationMock,
            [Frozen] PollingJobStub jobStub,
            NotificationHub hub)
        {
            await jobStub.Poll();

            notificationMock.Verify(self => self.CompleteAsync(), Times.AtLeastOnce());
        }

        [Theory, NotificationHubData]
        public async Task PollingFunc_WhenNotificationProcessingFailed_RetriesNotification(
            [Frozen] Mock<INotificationListener> listenerMock,
            [Frozen] Mock<IReceivedNotification> notificationMock,
            [Frozen] PollingJobStub jobStub,
            NotificationHub hub)
        {
            listenerMock
                .Setup(self => self.On(It.IsAny<EventStreamUpdated>()))
                .Throws<NotImplementedException>();

            await jobStub.Poll();

            notificationMock.Verify(self => self.RetryAsync(), Times.AtLeastOnce());
        }

        [Theory, NotificationHubData(hasSubscriber: false, startHub: false)]
        public void StartNotificationProcessing_WhenListenerListIsEmpty_DoesNotStartsPoller(
            [Frozen] PollingJobStub jobStub,
            IEventStoreConnection connection,
            NotificationHub hub)
        {
            hub.StartNotificationProcessing(connection);

            Assert.False(jobStub.JobStarted);
            Assert.False(jobStub.JobStoped);
        }
    }
}
