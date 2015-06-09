using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamTests
    {
        [Theory, AutoMoqData]
        public async Task OpenReaderAsync_ReturnsReaderForSpecifiedEventStream(
            [Frozen] EventStream eventStream,
            string streamName)
        {
            var reader = await eventStream.OpenReaderAsync(streamName);

            Assert.Equal(streamName, reader.StreamName);
        }
    }
}
