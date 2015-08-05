using System;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventConsumingNotificationListenerDataAttribute : AutoMoqDataAttribute
    {
        public EventConsumingNotificationListenerDataAttribute(bool throwException = false)
        {
            Fixture.Customize(new EventStreamConsumerMoqCustomization(false));

            Fixture.Customize<EventConsumingNotificationListenerStub>(composer => composer
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
