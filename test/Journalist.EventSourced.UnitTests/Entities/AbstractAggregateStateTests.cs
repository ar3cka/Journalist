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
        }

        [Fact]
        public void Mutate_IncrementsStateVersionWithNumberEvents()
        {
            var originalStateVersion = State.MutatedStateVersion;
            var events = Fixture.CreateMany<object>();

            foreach (var e in events)
            {
                State.Mutate(e);
            }

            Assert.Equal(originalStateVersion, State.OriginalStateVersion);
            Assert.Equal(originalStateVersion + events.Count(), State.MutatedStateVersion);
        }

        [Fact]
        public void Mutate_AppendsAllEventsToStateChanges()
        {
            var events = Fixture.CreateMany<object>();

            foreach (var e in events)
            {
                State.Mutate(e);
            }

            foreach (var e in events)
            {
                Assert.Contains(e, State.Changes);
            }
        }

        [Fact]
        public void StateWasPersisted_ResetsChanges()
        {
            State.Mutate(Fixture.Create<object>());

            State.StateWasPersisted(1);

            Assert.Empty(State.Changes);
        }

        [Fact]
        public void StateWasPersisted_SavesMutatedVersion()
        {
            State.Mutate(Fixture.Create<object>());

            State.StateWasPersisted(1);

            Assert.Equal(State.MutatedStateVersion, State.OriginalStateVersion);
        }

        [Fact]
        public void StateWasPersisted_WhenSavedVersionIsDifferentThanPersisted_Throws()
        {
            State.Mutate(Fixture.Create<object>());

            Assert.Throws<ArgumentException>(() => State.StateWasPersisted(Fixture.Create<int>()));
        }

        public IFixture Fixture { get; set; }

        public IAggregatePersistenceState State { get; set; }

        public class TestableState : AbstractAggregateState
        {
            public void When(object e)
            {
            }
        }
    }
}
