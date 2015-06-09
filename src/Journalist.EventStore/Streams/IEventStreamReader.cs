namespace Journalist.EventStore.Streams
{
    public interface IEventStreamReader
    {
        string StreamName { get; }
    }
}
