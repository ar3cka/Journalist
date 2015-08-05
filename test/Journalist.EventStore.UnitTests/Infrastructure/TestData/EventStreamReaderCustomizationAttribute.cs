using System;
using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventStreamReaderCustomizationAttribute : AutoMoqDataAttribute
    {
        public EventStreamReaderCustomizationAttribute(
            bool hasEvents = true,
            bool completed = false,
            bool leaderPromotion = true,
            bool disableAutoCommit = false)
        {
            Fixture.Customize(new EventStreamReaderCustomization(completed, hasEvents));
            Fixture.Customize(new EventStreamConsumingSessionCustomization(leaderPromotion));

            Fixture.Customize<Func<StreamVersion, Task>>(composer => composer
                .FromFactory((CommitStreamVersionFMock mock) => mock.Invoke));

            Fixture.Customize<EventStreamConsumer>(composer => composer.FromFactory(
                () => new EventStreamConsumer(
                    Fixture.Create<EventStreamConsumerId>(),
                    Fixture.Create<IEventStreamReader>(),
                    Fixture.Create<IEventStreamConsumingSession>(),
                    !disableAutoCommit,
                    Fixture.Create<StreamVersion>(),
                    Fixture.Create<Func<StreamVersion, Task>>())));
        }
    }
}
