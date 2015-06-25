using System.Collections.Generic;

namespace Journalist.EventSourced.Entities
{
    public interface IAggregateState
    {
        void Mutate(object e);

        void Restore(IEnumerable<object> events);

        void StateWasPersisted(int persistedVersion);

        void StateWasRestored(int restoredVersion);

        IReadOnlyList<object> Changes { get; }

        int MutatedStateVersion { get; }

        int OriginalStateVersion { get; }
    }
}
