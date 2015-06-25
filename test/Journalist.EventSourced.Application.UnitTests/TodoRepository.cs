using Journalist.EventSourced.Application.Infrastructure;
using Journalist.EventSourced.Application.Infrastructure.Storage;

namespace Journalist.EventSourced.Application.UnitTests
{
    public class TodoRepository : AbstractAggregateRootRepository<Todo, TodoId, TodoState>
    {
        public TodoRepository(IAggregateStateStorage<TodoState> stateStorage) : base(stateStorage)
        {
        }

        protected override Todo RestoreAggregateRoot(TodoId identity, TodoState state)
        {
            return new Todo(state);
        }

        protected override Todo StateNotFound(TodoId identity)
        {
            return new Todo(identity);
        }
    }
}
