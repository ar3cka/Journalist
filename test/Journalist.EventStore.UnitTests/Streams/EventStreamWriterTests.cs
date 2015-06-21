using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamWriterTests
    {
        [Theory, AutoMoqData]
        public async Task AppendEvents_AppendEventsToJournal(
            [Frozen] EventStreamPosition position,
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamWriter writer,
            JournaledEvent[] events)
        {
            await writer.AppendEventsAsync(events);

            journalMock.Verify(journal => journal.AppendEventsAsync(
                writer.StreamName,
                position,
                It.Is<IReadOnlyCollection<JournaledEvent>>(e => e.Count == events.Length)));
        }

        [Theory, AutoMoqData]
        public async Task MoveToEndOfStreamAsync_ReadsEndOfStreamPositionFormJournal(
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamWriter writer)
        {
            await writer.MoveToEndOfStreamAsync();

            journalMock.Verify(journal => journal.ReadEndOfStreamPositionAsync(writer.StreamName));
        }

        [Theory, AutoMoqData]
        public void StreamPosition_ReturnsStreamVersionValue([Frozen] EventStreamPosition position, EventStreamWriter writer)
        {
            Assert.Equal(position.Version, writer.StreamVersion);
        }

        [Theory, AutoMoqData]
        public async Task StreamPosition_AfterWriteAsyncCall_UpdatedStreamVersionValue(
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamPosition position,
            EventStreamWriter writer,
            JournaledEvent[] events)
        {
            journalMock
                .Setup(self => self.AppendEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<EventStreamPosition>(),
                    It.IsAny<IReadOnlyCollection<JournaledEvent>>()))
                .Returns(position.YieldTask());

            await writer.AppendEventsAsync(events);

            Assert.Equal(position.Version, writer.StreamVersion);
        }
    }
}
