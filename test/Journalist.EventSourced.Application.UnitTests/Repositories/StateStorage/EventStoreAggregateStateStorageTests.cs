using System.Threading.Tasks;
using Journalist.EventSourced.Application.Repositories.StateStorage;
using Journalist.EventSourced.Application.Serialization;
using Journalist.EventSourced.Application.UnitTests.Infrastructure.TestData;
using Journalist.EventStore.Events;
using Journalist.Options;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventSourced.Application.UnitTests.Repositories.Storage
{
    public class EventStoreAggregateStateStorageTests
    {
        [Theory]
        [RepositoryTestData(hasEvents: false)]
        public async Task RestoreStateAsync_WhenStreamEmpty_ReturnsNone(
            TodoId todoId,
            EventStoreAggregateStateStorage<TodoState> storage)
        {
            var state = await storage.RestoreStateAsync(todoId);

            Assert.True(state.IsNone);
        }

        [Theory]
        [RepositoryTestData]
        public async Task RestoreStateAsync_WhenStreamNotEmpty_ReturnsRestoredState(
            TodoId todoId,
            EventStoreAggregateStateStorage<TodoState> storage)
        {
            var state = await storage.RestoreStateAsync(todoId);

            Assert.True(state.IsSome);
        }

        [Theory]
        [RepositoryTestData]
        public async Task RestoreStateAsync_UseEventSerializer(
            [Frozen] Mock<IEventSerializer> serializerMock,
            TodoId todoId,
            EventStoreAggregateStateStorage<TodoState> storage)
        {
            var state = await storage.RestoreStateAsync(todoId);

            serializerMock.Verify(
                self => self.Deserialize(It.IsAny<JournaledEvent>()),
                Times.Exactly(state.Select(s => s.OriginalStateVersion).GetOrDefault(0)));
        }
    }
}
