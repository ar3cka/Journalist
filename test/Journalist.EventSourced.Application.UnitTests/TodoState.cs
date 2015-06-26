using Journalist.EventSourced.Entities;

namespace Journalist.EventSourced.Application.UnitTests
{
    public class TodoState : AbstractAggregateState
    {
        public TodoId Id
        {
            get; set;
        }

        public void When(object change)
        {
        }
    }
}
