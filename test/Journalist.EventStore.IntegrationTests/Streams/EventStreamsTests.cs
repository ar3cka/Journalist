using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
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
