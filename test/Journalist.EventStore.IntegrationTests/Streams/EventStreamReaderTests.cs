using System;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamReaderTests
    {
        public EventStreamReaderTests()
        {
            Connection = EventStoreConnectionBuilder
                .Create(config => config.UseStorage("UseDevelopmentStorage=true", "TestEventJournal"))
                .Build();

            StreamName = "stream-" + Guid.NewGuid().ToString("N");
        }

        [Theory, AutoMoqData]
        public async Task Reader_WhenStreamNotExists_ReturnsReaderCompletedReaderInLastPosition(JournaledEvent[] events)
        {
            var reader = await Connection.CreateStreamReaderAsync(StreamName);

            Assert.True(reader.IsCompleted);
            Assert.False(reader.HasEvents);
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
