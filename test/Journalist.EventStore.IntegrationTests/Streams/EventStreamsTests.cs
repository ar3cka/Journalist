using System;
using System.Threading.Tasks;
using Journalist.EventStore.Configuration;
using Journalist.EventStore.Events;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamsTests
    {
        public EventStreamsTests()
        {
            Connection = EventStoreConnectionBuilder
                .Create(config => config
                    .UseStorage("UseDevelopmentStorage=true", "TestEventJournal"))
                .Build();

            StreamName = "stream-" + Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task CreatedWriter_WhenStreamIsNotExists_ReturnsStreamAtStartPosition()
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);

            Assert.Equal(0, writer.StreamPosition);
        }

        [Theory, AutoMoqData]
        public async Task CreatedWriter_AppendsEventsAndMovesPositionForward(JournaledEvent[] dummyEvents)
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);
            await writer.AppendEventsAsync(dummyEvents);

            Assert.Equal(dummyEvents.Length, writer.StreamPosition);
        }

        [Theory, AutoMoqData]
        public async Task CreatedReader_CanReadAppendedEvents(JournaledEvent[] dummyEvents)
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);
            await writer.AppendEventsAsync(dummyEvents);

            var reader = await Connection.CreateStreamReaderAsync(StreamName);
            await reader.ReadEventsAsync();

            Assert.Equal(dummyEvents.Length, reader.Events.Count);
            for (var i = 0; i < dummyEvents.Length; i++)
            {
                Assert.Equal(dummyEvents[i], reader.Events[i]);
            }
        }

        [Theory, AutoMoqData]
        public async Task CreatedReader_WhenOpenedFromPositionOfTheLastEvent_ReturnsOneEvent(JournaledEvent[] dummyEvents)
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);
            await writer.AppendEventsAsync(dummyEvents);

            var reader = await Connection.CreateStreamReaderAsync(StreamName, writer.StreamPosition);
            await reader.ReadEventsAsync();

            Assert.Equal(1, reader.Events.Count);
            Assert.Equal(dummyEvents[writer.StreamPosition - 1], reader.Events[0]);
        }

        [Theory, AutoMoqData]
        public async Task CreatedProducer_CanPublishEvents(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducer(StreamName);
            await producer.PublishAsync(dummyEvents);
        }

        [Theory, AutoMoqData]
        public async Task CreatedProducer_WhenUnderlyingStreamWasMovedAheadCanPublishEvents(JournaledEvent[] dummyEvents)
        {
            // both producer are staying on the one stream version
            var producer1 = await Connection.CreateStreamProducer(StreamName);
            var producer2 = await Connection.CreateStreamProducer(StreamName);

            await producer1.PublishAsync(dummyEvents);
            await producer2.PublishAsync(dummyEvents);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_CanReadPublishedEvents(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducer(StreamName);
            await producer.PublishAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumer(StreamName);
            await consumer.ReceiveEventsAsync();

            Assert.Equal(dummyEvents, consumer.EnumerateEvents());
        }

        public string StreamName
        {
            get;
            private set;
        }

        public IEventStoreConnection Connection { get; set; }
    }
}
