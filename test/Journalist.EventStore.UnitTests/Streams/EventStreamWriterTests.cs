using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Streams;
using Journalist.EventStore.Streams.Notifications;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams
{
    public class EventStreamWriterTests
    {
        [Theory, EventStreamWriterData]
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

        [Theory, EventStreamWriterData]
        public async Task AppendEvents_NotifyAboutStreamUpdates(
            [Frozen] Mock<INotificationHub> hubMock,
            EventStreamWriter writer,
            JournaledEvent[] events)
        {
            var fromVersion = writer.StreamVersion;
            await writer.AppendEventsAsync(events);
            var toVersion = writer.StreamVersion;

            hubMock.Verify(journal => journal.NotifyAsync(
                It.Is<EventStreamUpdated>(notification =>
                    notification.StreamName == writer.StreamName &&
                    notification.FromVersion == fromVersion &&
                    notification.ToVersion == toVersion)));
        }

        [Theory, EventStreamWriterData]
        public async Task AppendEvents_ShouldUseEventMutationPipeline(
            [Frozen] Mock<IEventMutationPipeline> pipelineMock,
            EventStreamWriter writer,
            JournaledEvent[] events)
        {
            await writer.AppendEventsAsync(events);

            pipelineMock.Verify(
                journal => journal.Mutate(It.Is<JournaledEvent>(e => events.Contains(e))),
                Times.Exactly(events.Length));
        }

        [Theory, EventStreamWriterData]
        public async Task AppendEvents_AppendsMutatedEventsToStream(
            [Frozen] Mock<IEventMutationPipeline> pipelineMock,
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamWriter writer,
            JournaledEvent[] events,
            JournaledEvent[] mutatedEvents)
        {
            var callNumber = 0;
            pipelineMock
                .Setup(self => self.Mutate(It.IsAny<JournaledEvent>()))
                .Returns(() => mutatedEvents[callNumber++]);

            await writer.AppendEventsAsync(events);

            journalMock.Verify(
                self => self.AppendEventsAsync(
                    It.IsAny<string>(),
                    It.IsAny<EventStreamPosition>(),
                    mutatedEvents));
        }

        [Theory, EventStreamWriterData]
        public async Task MoveToEndOfStreamAsync_ReadsEndOfStreamPositionFormJournal(
            [Frozen] Mock<IEventJournal> journalMock,
            EventStreamWriter writer)
        {
            await writer.MoveToEndOfStreamAsync();

            journalMock.Verify(journal => journal.ReadEndOfStreamPositionAsync(writer.StreamName));
        }

        [Theory, EventStreamWriterData]
        public void StreamPosition_ReturnsStreamVersionValue([Frozen] EventStreamPosition position, EventStreamWriter writer)
        {
            Assert.Equal(position.Version, writer.StreamVersion);
        }

        [Theory, EventStreamWriterData]
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
