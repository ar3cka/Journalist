﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.Persistence;
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
            StreamName = Fixture.Create("TestStream-");
        }

        [Fact]
        public async Task AppendEventsAsync_WriteEventsToEndOfStream()
        {
            // arrange
            var position = EventStreamHeader.Unknown;

            // act
            position = await AppendEventsAsync(batchNumber: 3);

            // assert
            Assert.Equal(StreamVersion.Create(30), position.Version);
        }

        [Fact]
        public async Task AppendEventsAsync_WhenAppendingToStartPositionTwice_Throw()
        {
            // act
            await AppendEventsAsync(header: EventStreamHeader.Unknown);

            await Assert.ThrowsAsync<EventStreamConcurrencyException>(
                async () => await AppendEventsAsync(header: EventStreamHeader.Unknown));
        }

        [Fact]
        public async Task AppendEventsAsync_WhenAppendingToSamePositionTwice_Throw()
        {
            // arrange
            var position = await AppendEventsAsync();

            // act
            await AppendEventsAsync(header: position);

           await Assert.ThrowsAsync<EventStreamConcurrencyException>(async () => await AppendEventsAsync(header: position));
        }

        [Fact]
        public async Task OpenEventStreamAsync_ReadPreviousCommitedEvents()
        {
            // arrange
            await AppendEventsAsync(50, 4);

            // act
            var events = await ReadEventsAsync();

            // assert
            Assert.Equal(200, events.Count);
        }

        [Fact]
        public async Task OpenEventStreamAsync_ReturnsCommitedEvent()
        {
            // arrange
            await AppendEventsAsync(50, 4);

            // act
            var events = await ReadEventsAsync();

            // assert
            Assert.True(events.All(e => e.CommitTime.IsSome));
            Assert.True(events.All(e => e.Offset.IsSome));
        }

        private async Task<EventStreamHeader> AppendEventsAsync(
            EventStreamHeader header,
            int batchSize = EVENTS_COUNT,
            int batchNumber = BATCH_NUMBER)
        {
            var batches = PrepareBatch(batchSize, batchNumber);

            var currentPosition = header;
            while (batches.Any())
            {
                currentPosition = await Journal.AppendEventsAsync(StreamName, currentPosition, batches.Dequeue());
            }

            return currentPosition;
        }

        private async Task<EventStreamHeader> AppendEventsAsync(
            int batchSize = EVENTS_COUNT,
            int batchNumber = BATCH_NUMBER)
        {
            var batches = PrepareBatch(batchSize, batchNumber);

            var currentPosition = await Journal.ReadStreamHeaderAsync(StreamName);
            while (batches.Any())
            {
                currentPosition = await Journal.AppendEventsAsync(StreamName, currentPosition, batches.Dequeue());
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

        private async Task<List<JournaledEvent>> ReadEventsAsync()
        {
            var stream = await Journal.OpenEventStreamCursorAsync(StreamName);

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

        public string StreamName { get; set; }

        public Fixture Fixture { get; set; }

        public EventJournal Journal { get; set; }
    }
}
