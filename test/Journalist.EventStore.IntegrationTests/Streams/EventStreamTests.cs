using System;
using System.Threading.Tasks;
using Journalist.EventStore.Streams;
using Journalist.EventStore.Streams.Configuration;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamTests
    {
        public EventStreamTests()
        {
            Stream = EventStreamBuilder
                .Create(config => config
                    .UseStorage("UseDevelopmentStorage=true", "TestEventJournal")
                    .UseJsonSerializer())
                .Build();

            StreamName = "stream-" + Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task OpenWriterAsync_WhenStreamIsNotExists_ReturnsStreamAtStartPosition()
        {
            var writer = await Stream.OpenWriterAsync(StreamName);

            Assert.Equal(0, writer.StreamPosition);
        }

        [Theory, AutoData]
        public async Task OpenedWriterAsync_AppendsEventsAndMovesPositionForward(DummyEvent[] dummyEvents)
        {
            var writer = await Stream.OpenWriterAsync(StreamName);
            await writer.AppendEvents(dummyEvents);

            Assert.Equal(dummyEvents.Length, writer.StreamPosition);
        }

        [Theory, AutoData]
        public async Task OpenedReader_CanReadAppendedEvents(DummyEvent[] dummyEvents)
        {
            var writer = await Stream.OpenWriterAsync(StreamName);
            await writer.AppendEvents(dummyEvents);

            var reader = await Stream.OpenReaderAsync(StreamName);
            await reader.ReadEventsAsync();

            Assert.Equal(dummyEvents.Length, reader.Events.Count);
            for (var i = 0; i < dummyEvents.Length; i++)
            {
                Assert.Equal(dummyEvents[i], reader.Events[i]);
            }
        }

        [Theory, AutoData]
        public async Task OpenedReader_WhenOpenedFromPositionOfTheLastEvent_ReturnsOneEvent(DummyEvent[] dummyEvents)
        {
            var writer = await Stream.OpenWriterAsync(StreamName);
            await writer.AppendEvents(dummyEvents);

            var reader = await Stream.OpenReaderAsync(StreamName, writer.StreamPosition);
            await reader.ReadEventsAsync();

            Assert.Equal(1, reader.Events.Count);
            Assert.Equal(dummyEvents[writer.StreamPosition - 1], reader.Events[0]);
        }

        public string StreamName
        {
            get;
            private set;
        }

        public IEventStream Stream { get; set; }
    }
}
