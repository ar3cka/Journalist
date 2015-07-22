namespace Journalist.EventStore.Streams
{
    public interface IEventStreamInteractionEntity
    {
        string StreamName { get; }

        StreamVersion StreamVersion { get; }

        bool IsClosed { get; }
    }
}
