namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConnectivityState
    {
        void EnsureConnectionIsActive();
    }
}
