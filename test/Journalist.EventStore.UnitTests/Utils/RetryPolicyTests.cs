using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.EventStore.Utils;
using Journalist.EventStore.Utils.RetryPolicies;
using Xunit;

namespace Journalist.EventStore.UnitTests.Utils
{
    public class RetryPolicyTests
    {
        [Theory, AutoMoqData]
        public void AllowCall_WhenAttemptNumberIsEqualOrLessThenConfiguredValue_ReturnsTrue(RetryPolicy policy, int drift)
        {
           Assert.True(policy.AllowCall(policy.MaxAttemptNumber));
           Assert.True(policy.AllowCall(policy.MaxAttemptNumber - drift));
        }

        [Theory, AutoMoqData]
        public void AllowCall_WhenAttemptNumberIsGreaterThenConfiguredValue_ReturnsFalse(RetryPolicy policy, int drift)
        {
            Assert.False(policy.AllowCall(policy.MaxAttemptNumber + drift));
        }
    }
}
