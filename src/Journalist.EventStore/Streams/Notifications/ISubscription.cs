namespace Journalist.EventStore.Streams.Notifications
{
    public interface ISubscription
    {
        void Start();

        void Stop();
    }
}
