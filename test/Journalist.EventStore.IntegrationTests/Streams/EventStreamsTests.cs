using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamsTests
    {
        private const string INCOMING_HEADER_NAME = "incoming-header";
        private const string OUTGOING_HEADER_NAME = "outgoing-header";

        public EventStreamsTests()
        {
            Connection = EventStoreConnectionBuilder
                .Create(config => config
                    .UseStorage("UseDevelopmentStorage=true", "TestEventJournal")
                    .Mutate.IncomingEventsWith(new MessageMutator(INCOMING_HEADER_NAME))
                    .Mutate.OutgoingEventsWith(new MessageMutator(OUTGOING_HEADER_NAME)))
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
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents);
        }

        [Theory, AutoMoqData]
        public async Task CreatedProducer_WhenUnderlyingStreamWasMovedAheadCanPublishEvents(JournaledEvent[] dummyEvents)
        {
            // both producer are staying on the one stream version
            var producer1 = await Connection.CreateStreamProducerAsync(StreamName);
            var producer2 = await Connection.CreateStreamProducerAsync(StreamName);

            await producer1.PublishAsync(dummyEvents);
            await producer2.PublishAsync(dummyEvents);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_CanReadPublishedEvents(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer.ReceiveEventsAsync();
            var receivedEvents = consumer.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents, receivedEvents);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_CanReadPublishedEvents(JournaledEvent[] dummyEvents1, JournaledEvent[] dummyEvents2)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents1);

            var consumer = await Connection.CreateStreamConsumerAsync(StreamName);
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
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents1);

            var consumer1 = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer1.ReceiveEventsAsync();

            await producer.PublishAsync(dummyEvents2);
            var receivedEvents1 = consumer1.EnumerateEvents().ToList();
            await consumer1.ReceiveEventsAsync(); // saves position and stops reading.
            await consumer1.CloseAsync(); // frees session

            var consumer2 = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents2 = consumer2.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_SavesConsumedPositionPositionOnClose(JournaledEvent[] dummyEvents1, JournaledEvent[] dummyEvents2)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents1);

            var consumer1 = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer1.ReceiveEventsAsync();

            await producer.PublishAsync(dummyEvents2);
            var receivedEvents1 = consumer1.EnumerateEvents().ToList();
            await consumer1.CloseAsync(); // saves position and stops reading.

            var consumer2 = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents2 = consumer2.EnumerateEvents().ToList();
            await consumer2.CloseAsync();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_RememberConsumedVersion(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents);

            var consumer1 = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer1.ReceiveEventsAsync();
            foreach (var e in consumer1.EnumerateEvents())
            {
                await consumer1.CommitProcessedStreamVersionAsync();
                break;
            }
            await consumer1.CloseAsync(); // frees session


            var consumer2 = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents = consumer2.EnumerateEvents().ToList();
            await consumer2.CloseAsync();

            Assert.Equal(dummyEvents.Skip(1), receivedEvents);
        }

        [Theory, AutoMoqData]
        public async Task WriterAndReader_UseMutationPipelines(JournaledEvent[] dummyEvents)
        {
            var writer = await Connection.CreateStreamWriterAsync(StreamName);
            await writer.AppendEventsAsync(dummyEvents);

            var reader = await Connection.CreateStreamReaderAsync(StreamName);
            await reader.ReadEventsAsync();

            var events = reader.Events.ToList();
            for (var i = 0; i < events.Count; i++)
            {
                Assert.NotNull(events[i].Headers[INCOMING_HEADER_NAME]);
                Assert.NotNull(events[i].Headers[OUTGOING_HEADER_NAME]);
            }
        }

        [Theory, AutoMoqData]
        public async Task ConsumerAndProducers_UseMutationPipelines(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer.ReceiveEventsAsync();

            foreach (var journaledEvent in consumer.EnumerateEvents())
            {
                Assert.NotNull(journaledEvent.Headers[INCOMING_HEADER_NAME]);
                Assert.NotNull(journaledEvent.Headers[OUTGOING_HEADER_NAME]);
            }

            await consumer.CloseAsync();
        }

        public string StreamName
        {
            get; private set;
        }

        public IEventStoreConnection Connection
        {
            get; set;
        }
    }
}
