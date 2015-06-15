using System.Collections.Generic;

namespace Journalist.EventSourced.Entities
{
    public interface IAggregatePersistenceState
    {
        void Mutate(object e);

        void Restore(IEnumerable<object> events);

        void StateWasPersisted(int persistedVersion);

        IReadOnlyList<object> Changes { get; }

        int MutatedStateVersion { get; }

        int OriginalStateVersion { get; }
    }
}
