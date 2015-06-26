using System.Threading.Tasks;
using Journalist.EventSourced.Application.Repositories.Storage;
using Xunit;

namespace Journalist.EventSourced.Application.UnitTests.Repositories.Storage
{
    public class EventStoreAggregateStateStorageTests
    {
        [Theory]
        public async Task FactMethodName(
            TodoId todoId,
            EventStoreAggregateStateStorage<TodoState> state)
        {
            await state.RestoreStateAsync(todoId);
        }
    }
}
