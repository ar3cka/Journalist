using Journalist.EventStore.IntegrationTests.Infrastructure.Customizations.Customizations;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace Journalist.EventStore.IntegrationTests.Infrastructure.TestData
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Fixture()
                .Customize(new AutoConfiguredMoqCustomization())
                .Customize(new JournaledEventCustomization()))
        {

        }
    }
}
