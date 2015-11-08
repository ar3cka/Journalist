using Journalist.EventStore.Utils.RetryPolicies;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class RetryPolicyStub : IRetryPolicy
    {
        private int m_maxAttemptNumber = 2;

        public bool AllowCall(int attemptNumber)
        {
            return new RetryPolicy(m_maxAttemptNumber).AllowCall(attemptNumber);
        }

        public void ConfigureMaxAttemptNumber(int value)
        {
            m_maxAttemptNumber = value;
        }
    }
}
