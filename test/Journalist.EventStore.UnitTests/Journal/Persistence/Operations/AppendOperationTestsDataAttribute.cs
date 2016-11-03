using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Operations
{
    public class AppendOperationTestsDataAttribute : AutoMoqDataAttribute
    {
        public AppendOperationTestsDataAttribute()
        {
            Fixture.Customize<EventStreamHeader>(composer => composer
                .FromFactory((string etag, StreamVersion version) =>
                    IsNewStream
                        ? EventStreamHeader.Unknown
                        : new EventStreamHeader(etag, version)));
        }

        public bool IsNewStream { get; set; }
    }
}
