using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Infrastructure.Customizations
{
    public class EventStreamReaderCustomization : ICustomization
    {
        public EventStreamReaderCustomization(bool completed, bool hasEvents)
        {
            Completed = completed;
            HasEvents = hasEvents;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize(new JournaledEventCustomization());

            fixture.Customize<Mock<IEventStreamReader>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.StreamName)
                    .ReturnsUsingFixture(fixture))
                .Do(mock => mock
                    .Setup(self => self.HasEvents)
                    .Returns(HasEvents))
                .Do(mock => mock
                    .Setup(self => self.ReadEventsAsync())
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.StreamVersion)
                    .ReturnsUsingFixture(fixture))
                .Do(mock => mock
                    .Setup(self => self.Events)
                    .ReturnsUsingFixture(fixture))
                .Do(mock => mock
                    .Setup(self => self.ReaderStreamVersion)
                    .Returns(() => fixture.Create<StreamVersion>().Increment())));
        }

        public bool Completed { get; private set; }

        public bool HasEvents { get; private set; }
    }
}
