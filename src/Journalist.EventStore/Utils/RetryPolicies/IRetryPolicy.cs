namespace Journalist.EventStore.Utils.RetryPolicies
{
    public interface IRetryPolicy
    {
        bool AllowCall(int attemptNumber);
    }
}
