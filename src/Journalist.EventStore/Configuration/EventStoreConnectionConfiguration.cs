using System;
using System.Collections.Generic;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.Extensions;

namespace Journalist.EventStore.Configuration
{
    public class EventStoreConnectionConfiguration : IEventStoreConnectionConfiguration
    {
        private readonly IEventMutationPipelineConfiguration m_mutationPipelineConfiguration;
        private readonly INotificationProcessingConfiguration m_notificationProcessingConfiguration;
        private readonly List<IEventMutator> m_incomingMessageMutators;
        private readonly List<IEventMutator> m_outgoingMessageMutators;
        private readonly List<INotificationListener> m_notificationListeners;

        public EventStoreConnectionConfiguration()
        {
            m_incomingMessageMutators = new List<IEventMutator>();
            m_outgoingMessageMutators = new List<IEventMutator>();
            m_notificationListeners = new List<INotificationListener>();

            m_mutationPipelineConfiguration = new EventMutationPipelineConfiguration(
                this,
                m_incomingMessageMutators,
                m_outgoingMessageMutators);

            m_notificationProcessingConfiguration = new NotificationProcessingConfiguration(
                this,
                m_notificationListeners);
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

            if (NotificationQueueName.IsNullOrEmpty())
            {
                throw new InvalidOperationException("NotificationQueueName is not specified.");
            }

            if (StreamConsumerSessionsBlobName.IsNullOrEmpty())
            {
                throw new InvalidOperationException("StreamConsumerSessionsBlobName is not specified.");
            }
        }

        public IEventStoreConnectionConfiguration UseStorage(
            string storageConnectionString,
            string journalTableName,
            string eventStoreDeploymentTableName,
            string notificationQueueName,
            int notificationQueuePartitionCount,
            string streamConsumerSessionsBlobName)
        {
            Require.NotEmpty(storageConnectionString, "storageConnectionString");
            Require.NotEmpty(journalTableName, "journalTableName");
            Require.NotEmpty(eventStoreDeploymentTableName, "eventStoreDeploymentTableName");
            Require.NotEmpty(notificationQueueName, "notificationQueueName");
            Require.Positive(notificationQueuePartitionCount, "notificationQueuePartitionCount");
            Require.NotEmpty(streamConsumerSessionsBlobName, "streamConsumerSessionsBlobName");

            StorageConnectionString = storageConnectionString;
            JournalTableName = journalTableName;
            EventStoreDeploymentTableName = eventStoreDeploymentTableName;
            NotificationQueueName = notificationQueueName;
            NotificationQueuePartitionCount = notificationQueuePartitionCount;
            StreamConsumerSessionsBlobName = streamConsumerSessionsBlobName;

            return this;
        }

        public IEventMutationPipelineConfiguration Mutate
        {
            get { return m_mutationPipelineConfiguration; }
        }

        public INotificationProcessingConfiguration Notifications
        {
            get { return m_notificationProcessingConfiguration; }
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

        public string EventStoreDeploymentTableName
        {
            get;
            private set;
        }

        public string NotificationQueueName
        {
            get;
            private set;
        }

        public int NotificationQueuePartitionCount
        {
            get; private set;
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

        public IReadOnlyCollection<INotificationListener> NotificationListeners
        {
            get { return m_notificationListeners; }
        }
    }
}
