using System.Collections.Generic;

namespace Journalist.EventSourced.Entities
{
    public class AbstractAggregateState : IAggregatePersistenceState
    {
        private readonly List<object> m_changes = new List<object>();
        private int m_stateVersion;

        public void Mutate(object e)
        {
            Require.NotNull(e, "e");

            (this as dynamic).When((dynamic)e);

            m_stateVersion++;
            m_changes.Add(e);
        }

        public void Restore(IEnumerable<object> events)
        {
            Require.NotNull(events, "events");
        }

        public IReadOnlyList<object> Changes
        {
            get { return m_changes; }
        }

        public int StateVersion
        {
            get { return m_stateVersion; }
        }
    }
}
