using System.Threading.Tasks;
using Journalist.EventSourced.Application.Repositories;
using Journalist.EventSourced.Application.UnitTests.Infrastructure.TestData;
using Journalist.Options;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventSourced.Application.UnitTests.Repositories
{
    public class AggregationRootRepositoryTests
    {
        [Theory]
        [TodoAggregateStateStorageData(emptyState: true)]
        public async Task TryLoadAsync_WhenAggregateStateNotFound_ReturnsNone(
            TodoRepository repository)
        {
            var todo = await repository.TryLoadAsync(TodoId.Create());

            Assert.Equal(Option.None(), todo);
        }

        [Theory]
        [TodoAggregateStateStorageData]
        public async Task TryLoadAsync_WhenAggregateStateFound_ReturnsSome(
            [Frozen] TodoState state,
            TodoRepository repository)
        {
            var todo = await repository.TryLoadAsync(TodoId.Create());

            Assert.Equal(state.OriginalStateVersion.MayBe(), todo.Select(t => t.State.OriginalStateVersion));
        }

        [Theory]
        [TodoAggregateStateStorageData]
        public async Task SavesAsync_UseStateStorage(
            [Frozen] Mock<IAggregateStateStorage<TodoState>> storageMock,
            Todo todo,
            TodoRepository repository)
        {
            await repository.SaveAsync(todo);

            storageMock.Verify(self => self.PersistAsync(todo.Id, todo.State));
        }
    }
}
