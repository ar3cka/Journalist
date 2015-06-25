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

            if (JournalTableName.IsNullOrEmpty())
            {
                throw new InvalidOperationException("StreamConsumerSessionsBlobName is not specified.");
            }
        }

        public IEventStoreConnectionConfiguration UseStorage(
            string storageConnectionString,
            string journalTableName,
            string streamConsumerSessionsBlobName)
        {
            Require.NotEmpty(storageConnectionString, "storageConnectionString");
            Require.NotEmpty(journalTableName, "journalTableName");
            Require.NotEmpty(streamConsumerSessionsBlobName, "streamConsumerSessionsBlobName");

            StorageConnectionString = storageConnectionString;
            JournalTableName = journalTableName;
            StreamConsumerSessionsBlobName = streamConsumerSessionsBlobName;

            return this;
        }

        public string StorageConnectionString { get; private set; }

        public string JournalTableName { get; private set; }

        public string StreamConsumerSessionsBlobName { get; private set; }
    }
}
