using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
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

        [Theory]
        [AutoMoqData]
        public void TwoEqualsHeader_AreSame(EventStreamHeader header)
        {
            var header1 = header;
            var header2 = new EventStreamHeader(header1.ETag, header1.Version);

            Assert.Equal(header1, header2);
            Assert.True(header1 == header2);
        }

        [Theory]
        [AutoMoqData]
        public void TwoNotEqualsHeader_AreNotSame(EventStreamHeader header1, EventStreamHeader header2)
        {
            Assert.NotEqual(header1, header2);
            Assert.False(header1 == header2);
        }

        [Theory]
        [AutoMoqData]
        public void TwoHeadersWithDifferentETagValue_AreNotSame(string etag1, string etag2, StreamVersion version)
        {
            var header1 = new EventStreamHeader(etag1, version);
            var header2 = new EventStreamHeader(etag2, version);

            Assert.NotEqual(header1, header2);
            Assert.False(header1 == header2);
        }

        [Theory]
        [AutoMoqData]
        public void TwoHeadersWithDifferentVersionValue_AreNotSame(string etag, StreamVersion version1, StreamVersion version2)
        {
            var header1 = new EventStreamHeader(etag, version1);
            var header2 = new EventStreamHeader(etag, version2);

            Assert.NotEqual(header1, header2);
            Assert.False(header1 == header2);
        }

        [Fact]
        public void IsNewStream_ForUnknownHeader_ReturnsTrue()
        {
            Assert.True(EventStreamHeader.IsNewStream(EventStreamHeader.Unknown));
        }

        [Theory]
        [AutoMoqData]
        public void IsNewStream_ForNotUnknownHeader_ReturnsFalse(EventStreamHeader header)
        {
            Assert.False(EventStreamHeader.IsNewStream(header));
        }
    }
}
