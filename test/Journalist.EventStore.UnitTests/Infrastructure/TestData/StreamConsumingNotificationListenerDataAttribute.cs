using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
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
            Fixture.Customize(new EventStreamConsumerMoqCustomization(consumerReceivingFailed));
            Fixture.Customize(new EventStreamConsumerMoqCustomization(consumerReceivingFailed));

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
        }
    }
}
