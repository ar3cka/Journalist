namespace Journalist.EventStore.Utils
{
    public interface IRetryPolicy
    {
        bool AllowCall(int attemptNumber);
    }
}
