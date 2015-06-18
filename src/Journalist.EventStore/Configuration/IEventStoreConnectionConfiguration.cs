namespace Journalist.EventStore.Configuration
{
    public interface IEventStoreConnectionConfiguration
    {
        IEventStoreConnectionConfiguration UseStorage(string storageConnectionString, string journalTableName);
    }
}
