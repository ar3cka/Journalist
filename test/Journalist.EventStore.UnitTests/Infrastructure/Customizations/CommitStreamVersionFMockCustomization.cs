using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.Customizations
{
    public class CommitStreamVersionFMockCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Func<StreamVersion, Task>>(composer => composer
                .FromFactory((CommitStreamVersionFMock mock) => mock.Invoke));
        }
    }
}