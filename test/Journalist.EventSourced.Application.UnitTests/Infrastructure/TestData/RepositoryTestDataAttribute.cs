using Journalist.EventSourced.Application.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.TestHarness.Customizations;

namespace Journalist.EventSourced.Application.UnitTests.Infrastructure.TestData
{
    public class RepositoryTestDataAttribute : AutoMoqDataAttribute
    {
        public RepositoryTestDataAttribute(bool hasEvents = true)
        {
            Fixture.Customize(new JournaledEventCustomization());
            Fixture.Customize(new EventStoreConnectionCustomization(hasEvents));
        }
    }
}