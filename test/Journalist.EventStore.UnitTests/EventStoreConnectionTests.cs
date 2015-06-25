using System.Threading.Tasks;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests
{
    public class EventStoreConnectionTests
    {
        [Theory, AutoMoqData]
        public async Task OpenReaderAsync_ReturnsReaderForSpecifiedEventStream(
            [Frozen] EventStoreConnection eventStoreConnection,
            string streamName)
        {
            var reader = await eventStoreConnection.CreateStreamReaderAsync(streamName);

            Assert.Equal(streamName, reader.StreamName);
        }
    }
}
