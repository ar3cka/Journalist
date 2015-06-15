using System;
using System.Linq;
using Journalist.EventSourced.Entities;
using Ploeh.AutoFixture;
using Xunit;

namespace Journalist.EventSourced.UnitTests.Entities
{
    public class AbstractAggregateStateTests
    {
        public AbstractAggregateStateTests()
        {
            Fixture = new Fixture();
            State = Fixture.Create<TestableState>();
            Changes = Fixture.CreateMany<object>().ToArray();
        }

        [Fact]
        public void Mutate_IncrementsStateVersionWithNumberEvents()
        {
            var originalStateVersion = State.MutatedStateVersion;

            foreach (var e in Changes)
            {
                State.Mutate(e);
            }

            Assert.Equal(originalStateVersion, State.OriginalStateVersion);
            Assert.Equal(originalStateVersion + Changes.Count(), State.MutatedStateVersion);
        }

        [Fact]
        public void Mutate_AppendsAllEventsToStateChanges()
        {
            foreach (var e in Changes)
            {
                State.Mutate(e);
            }

            foreach (var e in Changes)
            {
                Assert.Contains(e, State.Changes);
            }
        }

        [Fact]
        public void StateWasPersisted_ResetsChanges()
        {
            State.Mutate(Changes);

            State.StateWasPersisted(1);

            Assert.Empty(State.Changes);
        }

        [Fact]
        public void StateWasPersisted_SavesMutatedVersion()
        {
            State.Mutate(Changes);

            State.StateWasPersisted(1);

            Assert.Equal(State.MutatedStateVersion, State.OriginalStateVersion);
        }

        [Fact]
        public void StateWasPersisted_WhenSavedVersionIsDifferentThanPersisted_Throws()
        {
            State.Mutate(Changes);

            Assert.Throws<ArgumentException>(() => State.StateWasPersisted(Fixture.Create<int>()));
        }

        [Fact]
        public void StateWasRestored_WhenNoChangesAppliedToState_Throws()
        {
            State.Mutate(Fixture.Create<object>());
            State.StateWasPersisted(State.MutatedStateVersion);

            Assert.Throws<InvalidOperationException>(() => State.StateWasRestored(State.MutatedStateVersion));
        }

        [Fact]
        public void StateWasRestored_WhenRestoredVersionIsDifferent_Throws()
        {
            State.Restore(Changes);

            Assert.Throws<ArgumentException>(() => State.StateWasRestored(Fixture.Create<int>()));
        }

        [Fact]
        public void StateWasRestored_SavesMutatedVersion()
        {
            State.Restore(Changes);

            State.StateWasRestored(State.OriginalStateVersion + Changes.Count());

            Assert.Equal(State.MutatedStateVersion, State.OriginalStateVersion);
        }

        [Fact]
        public void Restore_UpdatesMutatedVersion()
        {
            var originalVersion = State.OriginalStateVersion;

            State.Restore(Changes);

            Assert.Equal(originalVersion, State.OriginalStateVersion);
            Assert.Equal(State.OriginalStateVersion + Changes.Count(), State.MutatedStateVersion);
        }

        public IFixture Fixture { get; set; }

        public IAggregatePersistenceState State { get; set; }

        public object[] Changes { get; set; }

        public class TestableState : AbstractAggregateState
        {
            public void When(object e)
            {
            }
        }
    }
}
