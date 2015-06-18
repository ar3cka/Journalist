using Journalist.EventStore.Streams.Serializers;

namespace Journalist.EventStore.Configuration
{
    public interface IEventStoreConnectionConfiguration
    {
        IEventStoreConnectionConfiguration UseStorage(string storageConnectionString, string journalTableName);

        IEventStoreConnectionConfiguration UseSerializer(IEventSerializer serializer);
    }
}
