using System;
using System.Threading.Tasks;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventStreamReaderCustomizationAttribute : AutoMoqDataAttribute
    {
        public EventStreamReaderCustomizationAttribute(
            bool hasEvents = true,
            bool completed = false,
            bool leaderPromotion = true)
        {
            Fixture.Customize(new EventStreamReaderCustomization(completed, hasEvents));
            Fixture.Customize(new EventStreamConsumingSessionCustomization(leaderPromotion));

            Fixture.Customize<Func<StreamVersion, Task>>(composer => composer
                .FromFactory((CommitStreamVersionFMock mock) => mock.Invoke));
        }
    }
}
