using System;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Listeners
{
    public class StreamConsumingNotificationListenerTests
    {
        [Theory, StreamConsumingNotificationListenerData(startedSubscription: false)]
        public async Task OnEventStreamUpdated_WhenSubscriptionWasNotStarted_Throws(
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => listener.On(notification));
        }

        [Theory, StreamConsumingNotificationListenerData]
        public void OnSubscriptionStarted_WhenSubscriptionWasStarted_Throws(
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification,
            INotificationListenerSubscription subscription)
        {
            Assert.Throws<InvalidOperationException>(() => listener.OnSubscriptionStarted(subscription));
        }

        [Theory, StreamConsumingNotificationListenerData]
        public async Task nEventStreamUpdated_WhenSubscriptionWasStopped_Throws(
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification,
            INotificationListenerSubscription subscription)
        {
            listener.OnSubscriptionStopped();

            await Assert.ThrowsAsync<InvalidOperationException>(() => listener.On(notification));
        }

        [Theory, StreamConsumingNotificationListenerData]
        public async Task OnEventStreamUpdated_CreatesUpdatedStreamConsumer(
            [Frozen] Mock<INotificationListenerSubscription> subscriptionMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            await listener.On(notification);

            subscriptionMock.Verify(
                self => self.CreateSubscriptionConsumerAsync(notification.StreamName),
                Times.Once());
        }

        [Theory, StreamConsumingNotificationListenerData(consumerReceivingFailed: true)]
        public async Task OnEventStreamUpdated_WhenConsumerReceiveOperationFailed_DefersNotificationProcessing(
            [Frozen] Mock<INotificationListenerSubscription> subscriptionMock,
            [Frozen] Mock<IEventStreamConsumer> consumerMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            consumerMock
                .Setup(self => self.ReceiveEventsAsync())
                .Returns(TaskDone.False);

            await listener.On(notification);

            subscriptionMock.Verify(
                self => self.DefferNotificationAsync(notification),
                Times.Once());
        }

        [Theory, StreamConsumingNotificationListenerData(consumerReceivingFailed: true)]
        public async Task OnEventStreamUpdated_WhenConsumerReceiveOperationFailed_ClosesConsumer(
            [Frozen] Mock<IEventStreamConsumer> consumerMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            consumerMock
                .Setup(self => self.ReceiveEventsAsync())
                .Returns(TaskDone.False);

            await listener.On(notification);

            consumerMock.Verify(self => self.CloseAsync(), Times.Once());
        }

        [Theory, StreamConsumingNotificationListenerData(processingFailed: true)]
        public async Task OnEventStreamUpdated_WhenListenerProcessingFailed_DefersNotificationProcessing(
            [Frozen] Mock<INotificationListenerSubscription> subscriptionMock,
            [Frozen] Mock<IEventStreamConsumer> consumerMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            listener.ProcessingCompleted = false;

            await listener.On(notification);

            subscriptionMock.Verify(
                self => self.DefferNotificationAsync(notification),
                Times.Once());
        }

        [Theory, StreamConsumingNotificationListenerData(processingFailed: true)]
        public async Task OnEventStreamUpdated_WhenListenerProcessingFailed_ClosesConsumer(
            [Frozen] Mock<IEventStreamConsumer> consumerMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            await listener.On(notification);

            consumerMock.Verify(self => self.CloseAsync(), Times.Once());
        }

        [Theory, StreamConsumingNotificationListenerData]
        public async Task OnEventStreamUpdated_WhenListenerProcessingCompleted_CommitsConsumedStreamVersion(
            [Frozen] Mock<IEventStreamConsumer> consumerMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            await listener.On(notification);

            consumerMock.Verify(self => self.CommitProcessedStreamVersionAsync(false));
        }

        [Theory, StreamConsumingNotificationListenerData]
        public async Task OnEventStreamUpdated_WhenListenerProcessingCompleted_DoesNotDefereNotification(
            [Frozen] Mock<INotificationListenerSubscription> subscriptionMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            await listener.On(notification);

            subscriptionMock.Verify(self => self.DefferNotificationAsync(notification), Times.Never());
        }

        [Theory, StreamConsumingNotificationListenerData]
        public async Task OnEventStreamUpdated_WhenListenerProcessingCompleted_ClosesConsumer(
            [Frozen] Mock<IEventStreamConsumer> consumerMock,
            StreamConsumingNotificationListenerStub listener,
            EventStreamUpdated notification)
        {
            await listener.On(notification);

            consumerMock.Verify(self => self.CloseAsync());
        }
    }
}
