using Journalist.EventStore.Connection;
using Journalist.EventStore.Journal;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.Tasks;
using Moq;
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

            Fixture.Customize<Mock<IEventJournalReaders>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.IsRegisteredAsync(It.IsAny<EventStreamReaderId>()))
                    .Returns(() => newReader ? TaskDone.False : TaskDone.True))
                .Do(mock => mock
                    .Setup(self => self.RegisterAsync(It.IsAny<EventStreamReaderId>()))
                    .Returns(TaskDone.Done)));

            Fixture.Customize<EventStreamConsumerConfiguration>(composer => composer
                .Do(config =>
                {
                    config.ReadStream(Fixture.Create("StreamName"), readFromEnd);
                }));

        }
    }
}
