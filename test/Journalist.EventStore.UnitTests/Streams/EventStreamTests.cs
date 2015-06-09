using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Utils;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamTests
    {
        [Theory, AutoMoqData]
        public async Task Test([Frozen] EventStream eventStream, string streamName)
        {
            var reader = await eventStream.OpenReaderAsync(streamName);
        }
    }
}
