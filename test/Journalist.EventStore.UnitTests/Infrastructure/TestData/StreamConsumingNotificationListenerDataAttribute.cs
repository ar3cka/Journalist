using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class StreamConsumingNotificationListenerDataAttribute : AutoMoqDataAttribute
    {
        public StreamConsumingNotificationListenerDataAttribute(
            bool startedSubscription = true,
            bool processingFailed = false,
            bool consumerReceivingFailed = false)
        {
            Fixture.Customize<StreamConsumingNotificationListenerStub>(composer => composer
                .Do(stub =>
                {
                    if (startedSubscription)
                    {
                        stub.OnSubscriptionStarted(Fixture.Create<INotificationListenerSubscription>());
                    }

                    stub.ProcessingCompleted = !processingFailed;
                })
                .OmitAutoProperties());

            Fixture.Customize<Mock<IEventStreamConsumer>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.ReceiveEventsAsync())
                    .Returns(() => consumerReceivingFailed ? TaskDone.False : TaskDone.True))
                .Do(mock => mock
                    .Setup(self => self.CloseAsync())
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.CommitProcessedStreamVersionAsync(It.IsAny<bool>()))
                    .Returns(TaskDone.Done)));
        }
    }
}
