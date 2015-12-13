using Journalist.EventStore.Journal;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal
{
    public class EventStreamHeaderTests
    {
        [Fact]
        public void TwoUnknownHeaders_AreSame()
        {
            var header1 = EventStreamHeader.Unknown;
            var header2 = EventStreamHeader.Unknown;

            Assert.Equal(header1, header2);
            Assert.True(header1 == header2);
        }
    }
}
