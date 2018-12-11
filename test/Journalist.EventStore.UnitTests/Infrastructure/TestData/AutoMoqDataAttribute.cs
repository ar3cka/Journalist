using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Fixture()
                .Customize(new AutoConfiguredMoqCustomization())
                .Customize(new JournaledEventCustomization())
                .Customize(new IOStreamCustomization()))
        {
        }
    }
}
