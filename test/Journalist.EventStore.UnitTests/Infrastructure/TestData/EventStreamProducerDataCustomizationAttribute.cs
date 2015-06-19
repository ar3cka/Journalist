using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.Utils;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventStreamProducerDataCustomizationAttribute : AutoMoqDataAttribute
    {
        public EventStreamProducerDataCustomizationAttribute()
        {
            Fixture.Customize<IRetryPolicy>(composer => composer
                .FromFactory((RetryPolicyStub stub) => stub));
        }
    }
}
