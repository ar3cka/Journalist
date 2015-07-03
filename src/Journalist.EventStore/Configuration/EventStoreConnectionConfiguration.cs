using System;
using System.Collections.Generic;
using Journalist.EventStore.Events.Mutation;
using Journalist.Extensions;

namespace Journalist.EventStore.Configuration
{
    public class EventStoreConnectionConfiguration : IEventStoreConnectionConfiguration
    {
        private readonly IEventMutationPipelineConfiguration m_mutationPipelineConfiguration;
        private readonly List<IEventMutator> m_incomingMessageMutators;
        private readonly List<IEventMutator> m_outgoingMessageMutators;

        public EventStoreConnectionConfiguration()
        {
            m_incomingMessageMutators = new List<IEventMutator>();
            m_outgoingMessageMutators = new List<IEventMutator>();

            m_mutationPipelineConfiguration = new EventMutationPipelineConfiguration(
                this,
                m_incomingMessageMutators,
                m_outgoingMessageMutators);
        }

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

        public IEventMutationPipelineConfiguration Mutate
        {
            get { return m_mutationPipelineConfiguration; }
        }

        public string StorageConnectionString
        {
            get;
            private set;
        }

        public string JournalTableName
        {
            get;
            private set;
        }

        public string StreamConsumerSessionsBlobName
        {
            get;
            private set;
        }

        public IReadOnlyCollection<IEventMutator> IncomingMessageMutators
        {
            get { return m_incomingMessageMutators; }
        }

        public IReadOnlyCollection<IEventMutator> OutgoingMessageMutators
        {
            get { return m_outgoingMessageMutators; }
        }
    }
}
