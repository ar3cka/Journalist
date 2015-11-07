using System;
using Journalist.EventStore.Utils.Polling;
using Xunit;

namespace Journalist.EventStore.UnitTests.Utils.Polling
{
    public class PollingTimeoutTests
    {
        [Fact]
        public void Increase_WhenCallsThresholdWasHappend_Tests()
        {
            var timeout = new PollingTimeout();

            Assert.Equal(TimeSpan.FromSeconds(1), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(5), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(10), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(15), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(20), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(25), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(30), timeout.Value);

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(30), timeout.Value);
        }

        [Fact]
        public void Increase_WhenCallsThresholdWasNotHappend_Tests()
        {
            var timeout = new PollingTimeout();

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(1), timeout.Value);
        }

        [Fact]
        public void Reset_Tests()
        {
            var timeout = new PollingTimeout();

            Increase(timeout, PollingTimeout.INCREASING_THRESHOLD);
            Assert.Equal(TimeSpan.FromSeconds(5), timeout.Value);

            timeout.Reset();
            Assert.Equal(TimeSpan.FromSeconds(1), timeout.Value);
        }

        private static void Increase(IPollingTimeout timeout, int count)
        {
            for (var i = 0; i < count; i++)
            {
                timeout.Increase();
            }
        }
    }
}
