using System.IO;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.Customizations
{
    public class IOStreamCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Stream>(composer => composer
                .FromFactory((byte[] bytes) => new MemoryStream(bytes))
                .OmitAutoProperties());
        }
    }
}
