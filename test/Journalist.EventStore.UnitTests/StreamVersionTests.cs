using Journalist.EventStore.Events;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
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

        [Fact]
        public void IsUnknown_ForZeroVersion_ReturnsTrue()
        {
            var version = StreamVersion.Parse("0");

            Assert.True(StreamVersion.IsUnknown(version));
        }

        [Theory]
        [AutoMoqData]
        public void Increment_IncreasesVersion(int versionValue, int incrementValue)
        {
            var expectedVersion = StreamVersion.Create(versionValue + incrementValue);
            var version = StreamVersion.Create(versionValue);

            var incrementedVersion = version.Increment(incrementValue);

            Assert.Equal(expectedVersion, incrementedVersion);
        }

        [Theory]
        [AutoMoqData]
        public void Increment_IncreasesVersionByOne(int versionValue)
        {
            var expectedVersion = StreamVersion.Create(versionValue + 1);
            var version = StreamVersion.Create(versionValue);

            var incrementedVersion = version.Increment();

            Assert.Equal(expectedVersion, incrementedVersion);
        }

        [Theory]
        [AutoMoqData]
        public void Start_EqualsFirstVersion()
        {
            var expectedVersion = StreamVersion.Create(1);

            Assert.Equal(expectedVersion, StreamVersion.Start);
        }
    }
}
