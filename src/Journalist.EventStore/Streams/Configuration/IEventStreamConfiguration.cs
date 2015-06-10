using Journalist.EventStore.Streams.Serializers;

namespace Journalist.EventStore.Streams.Configuration
{
    public interface IEventStreamConfiguration
    {
        IEventStreamConfiguration UseStorage(string storageConnectionString, string journalTableName);

        IEventStreamConfiguration UseSerializer(IEventSerializer serializer);
    }
}
