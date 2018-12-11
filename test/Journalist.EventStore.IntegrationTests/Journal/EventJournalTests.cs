using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.Persistence;
using Journalist.Options;
using Journalist.WindowsAzure.Storage;
using Ploeh.AutoFixture;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Journal
{
    public class EventJournalTests
    {
        private const int EVENTS_COUNT = 10;
        private const int BATCH_NUMBER = 1;

        public EventJournalTests()
        {
            Fixture = PrepareFixture();
            Journal = PrepareEventJournal();
        }

        [Theory]
        [AutoMoqData]
        public async Task AppendEventsAsync_WriteEventsToEndOfStream(string streamName)
        {
            // arrange
            var position = EventStreamHeader.Unknown;

            // act
            position = await AppendEventsAsync(streamName, batchNumber: 3);

            // assert
            Assert.Equal(StreamVersion.Create(30), position.Version);
        }

        [Theory]
        [AutoMoqData]
        public async Task AppendEventsAsync_WhenAppendingToStartPositionTwice_Throw(string streamName)
        {
            // act
            await AppendEventsAsync(streamName, header: EventStreamHeader.Unknown);

            await Assert.ThrowsAsync<EventStreamConcurrencyException>(
                async () => await AppendEventsAsync(streamName, header: EventStreamHeader.Unknown));
        }

        [Theory]
        [AutoMoqData]
        public async Task AppendEventsAsync_WhenAppendingToSamePositionTwice_Throw(string streamName)
        {
            // arrange
            var position = await AppendEventsAsync(streamName);

            // act
            await AppendEventsAsync(streamName, header: position);

           await Assert.ThrowsAsync<EventStreamConcurrencyException>(async () => await AppendEventsAsync(streamName, header: position));
        }

        [Theory]
        [AutoMoqData]
        public async Task OpenEventStreamAsync_ReadPreviousCommitedEvents(string streamName)
        {
            // arrange
            await AppendEventsAsync(streamName, 50, 4);

            // act
            var events = await ReadEventsAsync(streamName);

            // assert
            Assert.Equal(200, events.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task OpenEventStreamWithLargeSliceSizeAsync_ReadPreviousCommitedEvents(string streamName)
        {
            // arrange
            await AppendEventsAsync(streamName, 50, 4);

            // act
            var events = await ReadEventsAsync(streamName, 1000);

            // assert
            Assert.Equal(200, events.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task OpenEventStreamFromAsync_ReadPreviousCommitedEvents(string streamName)
        {
            // arrange
            var fromVersion = StreamVersion.Create(101);
            await AppendEventsAsync(streamName, 50, 4);

            // act
            var events = await ReadEventsFromAsync(streamName, fromVersion);

            // assert
            Assert.Equal(100, events.Count);
            Assert.True(events[0].Offset.IsTrue(e => e == fromVersion));
            Assert.True(events[99].Offset.IsTrue(e => e == StreamVersion.Create(200)));
        }

        [Theory]
        [AutoMoqData]
        public async Task OpenEventStreamAsync_ReturnsCommitedEvent(string streamName)
        {
            // arrange
            await AppendEventsAsync(streamName);

            // act
            var events = await ReadEventsAsync(streamName);

            // assert
            Assert.True(events.All(e => e.CommitTime.IsSome));
            Assert.True(events.All(e => e.Offset.IsSome));
        }

        private async Task<EventStreamHeader> AppendEventsAsync(
            string streamName,
            EventStreamHeader header,
            int batchSize = EVENTS_COUNT,
            int batchNumber = BATCH_NUMBER)
        {
            var batches = PrepareBatch(batchSize, batchNumber);

            var currentPosition = header;
            while (batches.Any())
            {
                currentPosition = await Journal.AppendEventsAsync(streamName, currentPosition, batches.Dequeue());
            }

            return currentPosition;
        }

        private async Task<EventStreamHeader> AppendEventsAsync(
            string streamName,
            int batchSize = EVENTS_COUNT,
            int batchNumber = BATCH_NUMBER)
        {
            var batches = PrepareBatch(batchSize, batchNumber);

            var currentPosition = await Journal.ReadStreamHeaderAsync(streamName);
            while (batches.Any())
            {
                currentPosition = await Journal.AppendEventsAsync(streamName, currentPosition, batches.Dequeue());
            }

            return currentPosition;
        }

        private Queue<JournaledEvent[]> PrepareBatch(int batchSize, int batchNumber)
        {
            var batches = new Queue<JournaledEvent[]>();
            for (int i = 0; i < batchNumber; i++)
            {
                batches.Enqueue(Fixture.CreateMany<JournaledEvent>(batchSize).ToArray());
            }

            return batches;
        }

        private async Task<List<JournaledEvent>> ReadEventsAsync(string streamName, int sliceSize = 100)
        {
            var stream = await Journal.OpenEventStreamCursorAsync(streamName, sliceSize);

            var result = new List<JournaledEvent>();
            while (!stream.EndOfStream)
            {
                await stream.FetchSlice();
                result.AddRange(stream.Slice);
            }

            return result;
        }

        private async Task<List<JournaledEvent>> ReadEventsFromAsync(string streamName, StreamVersion fromVersion)
        {
            var stream = await Journal.OpenEventStreamCursorAsync(streamName, fromVersion);

            var result = new List<JournaledEvent>();
            while (!stream.EndOfStream)
            {
                await stream.FetchSlice();
                result.AddRange(stream.Slice);
            }

            return result;
        }

        private static EventJournal PrepareEventJournal()
        {
            var factory = new StorageFactory();
            var journal = new EventJournal(
                new EventJournalTable(factory.CreateTable("UseDevelopmentStorage=true", "TestEventJournal")));

            return journal;
        }

        private static Fixture PrepareFixture()
        {
            var fixture = new Fixture();

            fixture.Customize<JournaledEvent>(
                composer => composer.FromFactory((object stub) => JournaledEvent.Create(stub, Serialize)));

            return fixture;
        }

        private static void Serialize(object e, Type type, StreamWriter stream)
        {
            stream.Write("TestMessage");
        }

        public Fixture Fixture { get; set; }

        public EventJournal Journal { get; set; }
    }
}
