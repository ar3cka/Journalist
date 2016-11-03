namespace Journalist.EventStore.Utils.RetryPolicies
{
    public class RetryPolicy : IRetryPolicy
    {
        public static readonly IRetryPolicy Default = new RetryPolicy(100);

        private readonly int m_maxAttemptNumber;

        public RetryPolicy(int maxAttemptNumber)
        {
            Require.Positive(maxAttemptNumber, "maxAttemptNumber");

            m_maxAttemptNumber = maxAttemptNumber;
        }

        public bool AllowCall(int attemptNumber)
        {
            return attemptNumber <= m_maxAttemptNumber;
        }

        public int MaxAttemptNumber => m_maxAttemptNumber;
    }
}
