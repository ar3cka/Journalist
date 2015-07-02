using Journalist.EventStore.UnitTests.Infrastructure.Customizations;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EventStreamReaderDataAttribute : AutoMoqDataAttribute
    {
        public EventStreamReaderDataAttribute(bool emptyCursor = false)
        {
            Fixture.Customize(new EventStreamCursorCustomization(emptyCursor));
        }
    }
}