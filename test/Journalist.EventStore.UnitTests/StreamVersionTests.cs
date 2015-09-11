using Journalist.EventStore.Events;
using Xunit;

namespace Journalist.EventStore.UnitTests
{
    public class StreamVersionTests
    {
        [Fact]
        public void Parse_ForZero_ReturnsUnknownVersion()
        {
            var version = StreamVersion.Parse("0");

            Assert.Equal(StreamVersion.Unknown, version);
        }
    }
}
