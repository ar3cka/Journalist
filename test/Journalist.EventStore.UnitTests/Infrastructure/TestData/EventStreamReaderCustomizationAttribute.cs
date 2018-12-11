using System;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
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
            Fixture.Customize(new CommitStreamVersionFMockCustomization());

            Fixture.Customize<EventStreamConsumer>(composer => composer.FromFactory(
                () => new EventStreamConsumer(
                    Fixture.Create<IEventStreamConsumingSession>(),
                    Fixture.Create<IEventStreamReaderFactory>(),
                    !disableAutoCommit,
                    Fixture.Create<Func<StreamVersion, Task>>())));
        }
    }
}
