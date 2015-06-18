using System;
using Journalist.Extensions;

namespace Journalist.EventStore.Configuration
{
    public class EventStoreConnectionConfiguration : IEventStoreConnectionConfiguration
    {
        public void AssertConfigurationCompleted()
        {
            if (StorageConnectionString.IsNullOrEmpty())
            {
                throw new InvalidOperationException("StorageConnectionString is not specified.");
            }

            if (JournalTableName.IsNullOrEmpty())
            {
                throw new InvalidOperationException("JournalTableName is not specified.");
            }
        }

        public IEventStoreConnectionConfiguration UseStorage(string storageConnectionString, string journalTableName)
        {
            Require.NotEmpty(storageConnectionString, "storageConnectionString");
            Require.NotEmpty(journalTableName, "journalTableName");

            StorageConnectionString = storageConnectionString;
            JournalTableName = journalTableName;

            return this;
        }

        public string StorageConnectionString { get; private set; }

        public string JournalTableName { get; private set; }
    }
}
