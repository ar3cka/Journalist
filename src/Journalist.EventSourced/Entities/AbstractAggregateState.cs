using System.Collections.Generic;
using System.Net.Cache;

namespace Journalist.EventSourced.Entities
{
    public class AbstractAggregateState : IAggregatePersistenceState
    {
        private readonly List<object> m_changes = new List<object>();
        private int m_mutatedStateVersion;
        private int m_originalStateVersion;

        public void Mutate(object e)
        {
            Require.NotNull(e, "e");

            (this as dynamic).When((dynamic)e);

            m_mutatedStateVersion++;
            m_changes.Add(e);
        }

        public void Restore(IEnumerable<object> events)
        {
            Require.NotNull(events, "events");
        }

        public void StateWasPersisted(int persistedVersion)
        {
            Require.Positive(persistedVersion, "persistedVersion");
            Require.True(
                persistedVersion == m_mutatedStateVersion, 
                "persistedVersion", 
                "Persisted version is not equals to mutated.");

            m_changes.Clear();
            m_originalStateVersion = m_mutatedStateVersion;
        }

        public IReadOnlyList<object> Changes
        {
            get { return m_changes; }
        }

        public int MutatedStateVersion
        {
            get { return m_mutatedStateVersion; }
        }

        public int OriginalStateVersion
        {
            get { return m_originalStateVersion; }
        }
    }
}
