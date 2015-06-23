using Journalist.EventStore.Journal.StreamCursor;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class EmptyEventStreamCursorAttribute : AutoDataAttribute
    {
        public EmptyEventStreamCursorAttribute()
        {
            Fixture.Customize(new AutoConfiguredMoqCustomization());

            Fixture.Customize<EventStreamCursor>(composer => composer
                .FromFactory(() => EventStreamCursor.Empty));
        }
    }
}
