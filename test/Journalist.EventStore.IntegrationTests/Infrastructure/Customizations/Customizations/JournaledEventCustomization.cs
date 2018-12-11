using System;
using Journalist.EventStore.Events;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.IntegrationTests.Infrastructure.Customizations.Customizations
{
    public class JournaledEventCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<JournaledEvent>(composer => composer
                .FromFactory((Guid headerName, string headerValue) =>
                {
                    var result = JournaledEvent.Create(
                        new object(),
                        (_, type, writer) => writer.WriteLine(fixture.Create("EventPayload")));

                    result.SetHeader(headerName.ToString("N"), headerValue);

                    return result;
                }));
        }
    }
}
