using System;
using Journalist.EventStore.Notifications.Timeouts;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Timeouts
{
    public class PollingTimeoutTests
    {
        [Fact]
        public void Increase_Tests()
        {
            var timeout = new PollingTimeout();

            Assert.Equal(TimeSpan.FromSeconds(5), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(10), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(15), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(20), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(25), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(30), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(30), timeout.Value);
        }

        [Fact]
        public void Reset_Tests()
        {
            var timeout = new PollingTimeout();

            Assert.Equal(TimeSpan.FromSeconds(5), timeout.Value);

            timeout.Increase();
            Assert.Equal(TimeSpan.FromSeconds(10), timeout.Value);

            timeout.Reset();
            Assert.Equal(TimeSpan.FromSeconds(5), timeout.Value);
        }
    }
}
