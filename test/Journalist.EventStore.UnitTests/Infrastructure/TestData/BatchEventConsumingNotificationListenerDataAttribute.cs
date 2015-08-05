using System;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.UnitTests.Notifications.Listeners;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class BatchEventConsumingNotificationListenerDataAttribute : AutoMoqDataAttribute
    {
        public BatchEventConsumingNotificationListenerDataAttribute(bool throwException = false)
        {
            Fixture.Customize(new EventStreamConsumerMoqCustomization(false));

            Fixture.Customize<BatchEventConsumingNotificationListenerStub>(composer => composer
                .Do(stub =>
                {
                    stub.OnSubscriptionStarted(Fixture.Create<INotificationListenerSubscription>());

                    if (throwException)
                    {
                        stub.Exception = Fixture.Create<Exception>();
                    }

                }).OmitAutoProperties());
        }
    }
}