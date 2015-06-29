using System.IO;
using Journalist.EventStore.Events;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Events
{
    public class JournaledEventTests
    {
        [Theory]
        [AutoMoqData]
        public void GetEventPayload_ReturnsStreamWithPayloadContent(string payload)
        {
            var journaledEvent = JournaledEvent.Create(new object(), (evt, type, writer) => writer.Write(payload));

            using (var payloadStream = journaledEvent.GetEventPayload())
            using (var reader = new StreamReader(payloadStream))
            {
                Assert.Equal(payload, reader.ReadToEnd());
            }
        }

        [Theory]
        [AutoMoqData]
        public void GetEventPayload_ForRestoredEvents_ReturnsStreamWithPayloadContent(string payload)
        {
            var journaledEvent = JournaledEvent.Create(new object(), (evt, type, writer) => writer.Write(payload));

            var eventProperties = journaledEvent.ToDictionary();
            var restoredEvent = JournaledEvent.Create(eventProperties);

            using (var payloadStream = restoredEvent.GetEventPayload())
            using (var reader = new StreamReader(payloadStream))
            {
                Assert.Equal(payload, reader.ReadToEnd());
            }
        }
    }
}
