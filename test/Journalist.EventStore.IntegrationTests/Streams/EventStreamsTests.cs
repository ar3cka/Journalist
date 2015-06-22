using System;
using System.Linq;
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
        public async Task CreatedWriter_WhenStreamIsNotExists_ReturnsStreamAtUnknownPosition()
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);

            Assert.Equal(StreamVersion.Unknown, writer.StreamVersion);
        }

        [Theory, AutoMoqData]
        public async Task CreatedWriter_AppendsEventsAndMovesPositionForward(JournaledEvent[] dummyEvents)
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);
            await writer.AppendEventsAsync(dummyEvents);

            Assert.Equal(StreamVersion.Create(dummyEvents.Length), writer.StreamVersion);
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

            var reader = await Connection.CreateStreamReaderAsync(StreamName, writer.StreamVersion);
            await reader.ReadEventsAsync();

            Assert.Equal(1, reader.Events.Count);
            Assert.Equal(dummyEvents[(int)writer.StreamVersion - 1], reader.Events[0]);
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
            var receivedEvents = consumer.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents, receivedEvents);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_CanReadPublishedEvents(JournaledEvent[] dummyEvents1, JournaledEvent[] dummyEvents2)
        {
            var producer = await Connection.CreateStreamProducer(StreamName);
            await producer.PublishAsync(dummyEvents1);

            var consumer = await Connection.CreateStreamConsumer(StreamName);
            await consumer.ReceiveEventsAsync();
            var receivedEvents1 = consumer.EnumerateEvents().ToList();

            await producer.PublishAsync(dummyEvents2);
            await consumer.ReceiveEventsAsync();
            var receivedEvents2 = consumer.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_SavesConsumedPositionPosition(JournaledEvent[] dummyEvents1, JournaledEvent[] dummyEvents2)
        {
            var producer = await Connection.CreateStreamProducer(StreamName);
            await producer.PublishAsync(dummyEvents1);

            var consumer1 = await Connection.CreateStreamConsumer(StreamName);
            await consumer1.ReceiveEventsAsync();

            await producer.PublishAsync(dummyEvents2);
            var receivedEvents1 = consumer1.EnumerateEvents().ToList();
            await consumer1.ReceiveEventsAsync(); // saves position and stops reading.

            var consumer2 = await Connection.CreateStreamConsumer(StreamName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents2 = consumer2.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_SavesConsumedPositionPositionOnClose(JournaledEvent[] dummyEvents1, JournaledEvent[] dummyEvents2)
        {
            var producer = await Connection.CreateStreamProducer(StreamName);
            await producer.PublishAsync(dummyEvents1);

            var consumer1 = await Connection.CreateStreamConsumer(StreamName);
            await consumer1.ReceiveEventsAsync();

            await producer.PublishAsync(dummyEvents2);
            var receivedEvents1 = consumer1.EnumerateEvents().ToList();
            await consumer1.CloseAsync(); // saves position and stops reading.

            var consumer2 = await Connection.CreateStreamConsumer(StreamName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents2 = consumer2.EnumerateEvents().ToList();
            await consumer2.CloseAsync();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_RememberConsumedVersion(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducer(StreamName);
            await producer.PublishAsync(dummyEvents);

            var consumer1 = await Connection.CreateStreamConsumer(StreamName);
            await consumer1.ReceiveEventsAsync();
            foreach (var e in consumer1.EnumerateEvents())
            {
                await consumer1.RememberConsumedStreamVersionAsync();
                break;
            }

            var consumer2 = await Connection.CreateStreamConsumer(StreamName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents = consumer2.EnumerateEvents().ToList();
            await consumer2.CloseAsync();

            Assert.Equal(dummyEvents.Skip(1), receivedEvents);
        }

        public string StreamName
        {
            get;
            private set;
        }

        public IEventStoreConnection Connection { get; set; }
    }
}
