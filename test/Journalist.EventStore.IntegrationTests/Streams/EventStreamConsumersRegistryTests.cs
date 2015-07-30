using System.Threading.Tasks;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Journalist.EventStore.Streams;
using Journalist.WindowsAzure.Storage;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamConsumersRegistryTests
    {
        public EventStreamConsumersRegistryTests()
        {
            Registry = new EventStreamConsumersRegistry(
                new StorageFactory().CreateTable(
                "UseDevelopmentStorage=true",
                "EventStreamConsumersRegistryTests"));
        }

        [Theory, AutoMoqData]
        public async Task RegisterAsync_WhenConsumerNamesAreDifferent_ReturnsDifferentIds(string consumerName1, string consumerName2)
        {
            var consumerId1 = await Registry.RegisterAsync(consumerName1);
            var consumerId2 = await Registry.RegisterAsync(consumerName2);

            Assert.NotEqual(consumerId1, consumerId2);
        }

        [Theory, AutoMoqData]
        public async Task RegisterAsync_WhenConsumerNamesAreNotDifferent_ReturnsSameId(string consumerName)
        {
            var consumerId1 = await Registry.RegisterAsync(consumerName);
            var consumerId2 = await Registry.RegisterAsync(consumerName);

            Assert.Equal(consumerId1, consumerId2);
        }

        [Theory, AutoMoqData]
        public async Task IsResistedAsync_WhenConsumerWasNotRegistered_ReturnsFalse(EventStreamConsumerId consumerId)
        {
            var result = await Registry.IsResistedAsync(consumerId);

            Assert.False(result);
        }

        [Theory, AutoMoqData]
        public async Task IsResistedAsync_WhenConsumerWasRegistered_ReturnsTrue(string consumerName)
        {
            var consumerId = await Registry.RegisterAsync(consumerName);
            var result = await Registry.IsResistedAsync(consumerId);

            Assert.True(result);
        }

        public EventStreamConsumersRegistry Registry
        {
            get; set;
        }
    }
}
