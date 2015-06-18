using System;
using Journalist.EventStore.Streams.Serializers;
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

            if (EventSerializer == null)
            {
                throw new InvalidOperationException("EventSerializer is not configured.");
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

        public IEventStoreConnectionConfiguration UseSerializer(IEventSerializer serializer)
        {
            Require.NotNull(serializer, "serializer");

            EventSerializer = serializer;

            return this;
        }

        public string StorageConnectionString { get; private set; }

        public string JournalTableName { get; private set; }

        public IEventSerializer EventSerializer { get; private set; }
    }
}
