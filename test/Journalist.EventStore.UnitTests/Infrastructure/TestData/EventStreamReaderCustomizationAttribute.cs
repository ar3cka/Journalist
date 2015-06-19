using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations.Customizations;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventStreamReaderCustomizationAttribute : AutoMoqDataAttribute
    {
        public EventStreamReaderCustomizationAttribute()
        {
            Fixture.Customize(new JournaledEventCustomization());

            Fixture.Customize<Mock<IEventStreamReader>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.HasEvents)
                    .Returns(HasEvents))
                .Do(mock => mock
                    .Setup(self => self.ReadEventsAsync())
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.Events)
                    .ReturnsUsingFixture(Fixture)));
        }

        public bool HasEvents { get; set; }
    }
}
