using Journalist.EventSourced.Entities;

namespace Journalist.EventSourced.Application.UnitTests
{
    public class Todo : IAggregateRoot<TodoId, TodoState>
    {
        public Todo(TodoState state)
        {
            State = state;
        }

        public TodoId Id
        {
            get;
            private set;
        }

        public TodoState State
        {
            get;
            private set;
        }
    }
}
