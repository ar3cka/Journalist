using System.Collections.Generic;

namespace Journalist.EventSourced.Entities
{
    public interface IAggregatePersistenceState
    {
        void Mutate(object e);

        void Restore(IEnumerable<object> events);

        IReadOnlyList<object> Changes { get; }

        int StateVersion { get; }
    }
}
