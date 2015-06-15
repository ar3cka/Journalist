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
            var originalStateVersion = State.StateVersion;
            var events = Fixture.CreateMany<object>();

            foreach (var e in events)
            {
                State.Mutate(e);
            }


            Assert.Equal(originalStateVersion + events.Count(), State.StateVersion);
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
