namespace Journalist.EventStore.Streams
{
    public interface IEventStreamConnectivityState
    {
        void EnsureConnectionIsActive();

        bool IsActive { get; }

        bool IsClosing { get; }

        bool IsClosed { get; }
    }
}
