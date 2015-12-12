using System;
using System.Collections.Generic;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.Extensions;

namespace Journalist.EventStore.Configuration
{
    public class EventStoreConnectionConfiguration : IEventStoreConnectionConfiguration
    {
        private readonly EventMutationPipelineConfiguration m_mutationPipelineConfiguration;
        private readonly NotificationProcessingConfiguration m_notificationProcessingConfiguration;
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

            if (StreamConsumerSessionsBlobContainerName.IsNullOrEmpty())
            {
                throw new InvalidOperationException("StreamConsumerSessionsBlobContainerName is not specified.");
            }
        }

        public IEventStoreConnectionConfiguration UseStorage(
            string storageConnectionString,
            string journalTableName,
            string eventStoreDeploymentTableName,
            string notificationQueueName,
            int notificationQueuePartitionCount,
            string streamConsumerSessionsBlobContainerName,
            string pendingNotificationsChaserExclusiveAccessLockBlobContainerName,
            string pendingNotificationsChaserExclusiveAccessLockBlobName)
        {
            Require.NotEmpty(storageConnectionString, "storageConnectionString");
            Require.NotEmpty(journalTableName, "journalTableName");
            Require.NotEmpty(eventStoreDeploymentTableName, "eventStoreDeploymentTableName");
            Require.NotEmpty(notificationQueueName, "notificationQueueName");
            Require.Positive(notificationQueuePartitionCount, "notificationQueuePartitionCount");
            Require.NotEmpty(streamConsumerSessionsBlobContainerName, "streamConsumerSessionsBlobContainerName");
            Require.NotEmpty(pendingNotificationsChaserExclusiveAccessLockBlobContainerName, "pendingNotificationsChaserExclusiveAccessLockBlobContainerName");
            Require.NotEmpty(pendingNotificationsChaserExclusiveAccessLockBlobName, "pendingNotificationsChaserExclusiveAccessLockBlobName");

            StorageConnectionString = storageConnectionString;
            JournalTableName = journalTableName;
            EventStoreDeploymentTableName = eventStoreDeploymentTableName;
            NotificationQueueName = notificationQueueName;
            NotificationQueuePartitionCount = notificationQueuePartitionCount;
            StreamConsumerSessionsBlobContainerName = streamConsumerSessionsBlobContainerName;
            PendingNotificationsChaserExclusiveAccessLockBlobName = pendingNotificationsChaserExclusiveAccessLockBlobName;
            PendingNotificationsChaserExclusiveAccessLockBlobContainerName = pendingNotificationsChaserExclusiveAccessLockBlobContainerName;

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
            get;
            private set;
        }

        public string StreamConsumerSessionsBlobContainerName
        {
            get;
            private set;
        }

        public string PendingNotificationsChaserExclusiveAccessLockBlobName
        {
            get;
            private set;
        }

        public string PendingNotificationsChaserExclusiveAccessLockBlobContainerName
        {
            get;
            private set;
        }

        public bool BackgroundProcessingEnabled
        {
            get { return m_notificationProcessingConfiguration.BackgroundProcessingEnabled; }
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
