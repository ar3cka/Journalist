using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Journalist.EventStore.Streams;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamConsumerTests
    {
        public EventStreamConsumerTests()
        {
            Connection = EventStoreConnectionBuilder
                .Create(config => config.UseStorage("UseDevelopmentStorage=true", "TestEventJournal"))
                .Build();

            StreamName = "stream-" + Guid.NewGuid().ToString("N");
        }

        [Theory, AutoMoqData]
        public async Task CloseConsumer_CommitsConsumedVersion(
            JournaledEvent[] dummyEvents)
        {
            await PublishEventsAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer.ReceiveEventsAsync();
            await consumer.CloseAsync();

            consumer = await Connection.CreateStreamConsumerAsync(StreamName);

            Assert.Equal(ReceivingResultCode.EmptyStream, await consumer.ReceiveEventsAsync());
        }

        [Theory, AutoMoqData]
        public async Task ReceiveEventsAsync_ReadsPublishedEvents(
            JournaledEvent[] dummyEvents)
        {
            await PublishEventsAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumerAsync(StreamName);
            await consumer.ReceiveEventsAsync();
            var receivedEvents = consumer.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents, receivedEvents);
        }

        [Theory, AutoMoqData]
        public async Task ReceiveEventsAsync_WhenConsumerConfiguredStartReadingFromTheEnd_SkipsPreviousPublishedEvents(
            string consumerName,
            JournaledEvent[] dummyEvents)
        {
            await PublishEventsAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumerAsync(config => config
                .ReadStream(StreamName, true)
                .UseConsumerName(consumerName));

            Assert.Equal(ReceivingResultCode.EmptyStream, await consumer.ReceiveEventsAsync());
        }

        [Theory, AutoMoqData]
        public async Task ReceiveEventsAsync_WhenConsumerConfiguredStartReadingFromTheEnd_ContinuesReadingEndOfStream(
            string consumerName,
            JournaledEvent[] dummyEvents)
        {
            await PublishEventsAsync(dummyEvents);

            var consumer = await Connection.CreateStreamConsumerAsync(config => config
                .ReadStream(StreamName, true)
                .UseConsumerName(consumerName));

            await PublishEventsAsync(dummyEvents);

            consumer = await Connection.CreateStreamConsumerAsync(config => config
                .ReadStream(StreamName, true)
                .UseConsumerName(consumerName));

            Assert.Equal(ReceivingResultCode.EventsReceived, await consumer.ReceiveEventsAsync());
            Assert.Equal(dummyEvents, consumer.EnumerateEvents());
        }

        [Theory, AutoMoqData]
        public async Task CloseAsync_SavesConsumedPositionPosition(
            JournaledEvent[] dummyEvents1,
            JournaledEvent[] dummyEvents2,
            string consumerName)
        {
            await PublishEventsAsync(dummyEvents1);

            var consumer = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer.ReceiveEventsAsync();
            var receivedEvents1 = consumer.EnumerateEvents().ToArray();
            await consumer.CloseAsync(); // saves position

            await PublishEventsAsync(dummyEvents2);
            consumer = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer.ReceiveEventsAsync();
            var receivedEvents2 = consumer.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_WhenAutoCommitDisabled_DoesNotSaveConsumedPositionPosition(
            JournaledEvent[] dummyEvents1,
            JournaledEvent[] dummyEvents2,
            string consumerName)
        {
            await PublishEventsAsync(dummyEvents1);

            var consumer = await Connection.CreateStreamConsumerAsync(config => config
                .ReadStream(StreamName)
                .UseConsumerName(consumerName)
                .AutoCommitProcessedStreamPosition(false));

            await consumer.ReceiveEventsAsync();
            await consumer.CloseAsync();

            await PublishEventsAsync(dummyEvents2);
            consumer = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer.ReceiveEventsAsync();
            var receivedBatch = consumer.EnumerateEvents().ToList();

            Assert.Equal(dummyEvents1.Union(dummyEvents2), receivedBatch);
        }

        [Theory, AutoMoqData]
        public async Task CommitProcessedStreamVersionAsync_WhenAutoCommitDisabled_SavesConsumedPositionPosition(
            JournaledEvent[] dummyEvents1,
            JournaledEvent[] dummyEvents2,
            string consumerName)
        {
            await PublishEventsAsync(dummyEvents1);

            var consumer = await Connection.CreateStreamConsumerAsync(config => config
                .ReadStream(StreamName)
                .UseConsumerName(consumerName)
                .AutoCommitProcessedStreamPosition(false));

            await consumer.ReceiveEventsAsync();
            await consumer.CommitProcessedStreamVersionAsync();
            await consumer.CloseAsync();

            consumer = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            Assert.Equal(ReceivingResultCode.EmptyStream, await consumer.ReceiveEventsAsync());
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_SavesConsumedPositionPositionOnClose(
            JournaledEvent[] dummyEvents1,
            JournaledEvent[] dummyEvents2,
            string consumerName)
        {
            await PublishEventsAsync(dummyEvents1);

            var consumer1 = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer1.ReceiveEventsAsync();

            await PublishEventsAsync(dummyEvents2);
            var receivedEvents1 = consumer1.EnumerateEvents().ToList();
            await consumer1.CloseAsync(); // saves position and stops reading.

            var consumer2 = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents2 = consumer2.EnumerateEvents().ToList();
            await consumer2.CloseAsync();

            Assert.Equal(dummyEvents1, receivedEvents1);
            Assert.Equal(dummyEvents2, receivedEvents2);
        }

        [Theory, AutoMoqData]
        public async Task CreatedConsumer_RememberConsumedVersion(
            JournaledEvent[] dummyEvents,
            string consumerName)
        {
            await PublishEventsAsync(dummyEvents);

            var consumer1 = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer1.ReceiveEventsAsync();
            foreach (var e in consumer1.EnumerateEvents())
            {
                await consumer1.CommitProcessedStreamVersionAsync();
                break;
            }
            await consumer1.CloseAsync(); // frees session


            var consumer2 = await Connection.CreateStreamConsumerAsync(StreamName, consumerName);
            await consumer2.ReceiveEventsAsync();
            var receivedEvents = consumer2.EnumerateEvents().ToList();
            await consumer2.CloseAsync();

            Assert.Equal(dummyEvents.Skip(1), receivedEvents);
        }

        private async Task PublishEventsAsync(JournaledEvent[] dummyEvents)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(dummyEvents);
        }

        public string StreamName
        {
            get;
            private set;
        }

        public IEventStoreConnection Connection
        {
            get;
            private set;
        }
    }
}
