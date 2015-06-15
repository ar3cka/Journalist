using System.Collections.Generic;

namespace Journalist.EventSourced.Entities
{
    public interface IAggregateState
    {
        void Mutate(object e);

        void Restore(IEnumerable<object> events);

        int StateVersion { get; }
    }
}