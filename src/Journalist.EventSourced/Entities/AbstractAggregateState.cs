using System;
using System.Collections.Generic;

namespace Journalist.EventSourced.Entities
{
    public class AbstractAggregateState : IAggregatePersistenceState
    {
        private readonly List<object> m_changes = new List<object>();
        private int m_mutatedStateVersion;
        private int m_originalStateVersion;
        private bool m_mutating;
        private bool m_restoring;

        public void Mutate(object e)
        {
            Require.NotNull(e, "e");

            AssertStateIsBeingMutated();

            ApplyChange(e);

            m_changes.Add(e);
            m_mutating = true;
        }

        public void Restore(IEnumerable<object> events)
        {
            Require.NotNull(events, "events");

            AssertStateIsBeingRestored();

            foreach (var e in events)
            {
                ApplyChange(e);
            }

            m_restoring = true;
        }

        public void StateWasPersisted(int persistedVersion)
        {
            Require.Positive(persistedVersion, "persistedVersion");
            Require.True(
                persistedVersion == m_mutatedStateVersion,
                "persistedVersion",
                "Persisted version is not equals to mutated.");

            AssertStateIsBeingMutated();
            AsserStateHasAppliedChanges();

            m_changes.Clear();
            m_originalStateVersion = m_mutatedStateVersion;
            m_mutating = false;
        }

        public void StateWasRestored(int restoredVersion)
        {
            Require.Positive(restoredVersion, "restoredVersion");
            Require.True(
                restoredVersion == m_mutatedStateVersion,
                "restoredVersion",
                "Restored version is not equals to mutated.");

            AssertStateIsBeingRestored();
            AsserStateHasAppliedChanges();

            m_originalStateVersion = m_mutatedStateVersion;
            m_restoring = false;
        }

        private void ApplyChange(dynamic e)
        {
            (this as dynamic).When(e);
            m_mutatedStateVersion++;
        }

        private void AsserStateHasAppliedChanges()
        {
            if (m_mutatedStateVersion <= m_originalStateVersion)
            {
                throw new InvalidOperationException("There is no changes for restore from.");
            }
        }

        private void AssertStateIsBeingMutated()
        {
            if (m_restoring)
            {
                throw new InvalidOperationException("State is being restored.");
            }
        }

        private void AssertStateIsBeingRestored()
        {
            if (m_mutating)
            {
                throw new InvalidOperationException("State is being mutated.");
            }
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
