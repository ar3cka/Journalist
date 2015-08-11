using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Connection
{
    public class EventStreamConsumerStreamReaderFactoryTests
    {
        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: true, readFromEnd: false)]
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsFalse_CreatesReaderFromStart(
            [Frozen] Mock<IEventStoreConnection> connectionMock,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            connectionMock.Verify(
                self => self.CreateStreamReaderAsync(It.IsAny<string>(), StreamVersion.Start));
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: true, readFromEnd: true)]
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsTrue_CreatesReaderFromStreamVersion(
            [Frozen] Mock<IEventStoreConnection> connectionMock,
            [Frozen] StreamVersion streamVersion,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            connectionMock.Verify(
                self => self.CreateStreamReaderAsync(It.IsAny<string>(), streamVersion.Increment()));
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: true, readFromEnd: true)]
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsTrue_CommitsReaderVersion(
            [Frozen] CommitStreamVersionFMock commitFuncMock,
            [Frozen] StreamVersion streamVersion,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            Assert.Equal(1, commitFuncMock.CallsCount);
            Assert.Equal(streamVersion, commitFuncMock.CommitedVersion);
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: true, readFromEnd: false)]
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsFalse_CommitsReaderVersion(
            [Frozen] CommitStreamVersionFMock commitFuncMock,
            [Frozen] StreamVersion streamVersion,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            Assert.Equal(1, commitFuncMock.CallsCount);
            Assert.Equal(StreamVersion.Unknown, commitFuncMock.CommitedVersion);
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData()]
        public async Task CreateAsync_ForNotNewReader_DoesNotCommitReaderVersion(
            [Frozen] CommitStreamVersionFMock commitFuncMock,
            [Frozen] StreamVersion streamVersion,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            Assert.Equal(0, commitFuncMock.CallsCount);
        }
    }
}
