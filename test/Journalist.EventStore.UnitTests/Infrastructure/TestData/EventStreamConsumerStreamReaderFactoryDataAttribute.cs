using Journalist.EventStore.Connection;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventStreamConsumerStreamReaderFactoryDataAttribute : AutoMoqDataAttribute
    {
        public EventStreamConsumerStreamReaderFactoryDataAttribute(
            bool newReader = false,
            bool readFromEnd = false)
        {
            Fixture.Customize(new CommitStreamVersionFMockCustomization());

            Fixture.Customize<EventStreamConsumerStreamReaderFactory>(composer => composer.FromFactory(
                () => new EventStreamConsumerStreamReaderFactory(
                    Fixture.Create<IEventStoreConnection>(),
                    Fixture.Create("StreamName"),
                    readFromEnd,
                    newReader ? StreamVersion.Unknown : Fixture.Create<StreamVersion>(),
                    Fixture.Create<StreamVersion>(),
                    Fixture.Create<CommitStreamVersionFMock>().Invoke)));
        }
    }
}