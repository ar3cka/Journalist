using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
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
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsFalse_CommitsUnknownReaderVersion(
            [Frozen] EventStreamReaderId readerId,
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            journalMock.Verify(
                self => self.CommitStreamReaderPositionAsync(
                    It.IsAny<string>(),
                    readerId,
                    StreamVersion.Unknown));
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: true, readFromEnd: false)]
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsFalse_RegisterReader(
            [Frozen] EventStreamReaderId readerId,
            [Frozen] Mock<IEventJournalReaders> readersMock,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            readersMock.Verify(self => self.RegisterAsync(readerId));
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: true, readFromEnd: true)]
        public async Task CreateAsync_ForNewReaderWhenReadFromEndIsTrue_CreatesReaderFromStreamVersion(
            [Frozen] EventStreamReaderId readerId,
            [Frozen] Mock<IEventJournal> journalMock,
            [Frozen] StreamVersion streamVersion,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            journalMock.Verify(
                self => self.CommitStreamReaderPositionAsync(
                    It.IsAny<string>(),
                    readerId,
                    streamVersion));
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: false)]
        public async Task CreateAsync_ForExistingReader_DoesNotCommitVersion(
            [Frozen] EventStreamReaderId readerId,
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            journalMock.Verify(
                self => self.CommitStreamReaderPositionAsync(It.IsAny<string>(), readerId, It.IsAny<StreamVersion>()),
                Times.Never());
        }

        [Theory, EventStreamConsumerStreamReaderFactoryData(newReader: false)]
        public async Task CreateAsync_ForExistingReader_DoesNotRegisterReader(
            [Frozen] EventStreamReaderId readerId,
            [Frozen] Mock<IEventJournalReaders> readersMock,
            EventStreamConsumerStreamReaderFactory factory)
        {
            await factory.CreateAsync();

            readersMock.Verify(
                self => self.RegisterAsync(readerId),
                Times.Never());
        }
    }
}
