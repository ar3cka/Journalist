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
            string failedNotificationsTableName,
            string streamConsumerSessionsBlobContainerName,
            string pendingNotificationsTableName,
            string pendingNotificationsChaserExclusiveAccessLockBlobContainerName,
            string pendingNotificationsChaserExclusiveAccessLockBlobName)
        {
            Require.NotEmpty(storageConnectionString, nameof(storageConnectionString));
            Require.NotEmpty(journalTableName, nameof(journalTableName));
            Require.NotEmpty(eventStoreDeploymentTableName, nameof(eventStoreDeploymentTableName));
            Require.NotEmpty(notificationQueueName, nameof(notificationQueueName));
            Require.Positive(notificationQueuePartitionCount, nameof(notificationQueuePartitionCount));
            Require.NotEmpty(failedNotificationsTableName, nameof(failedNotificationsTableName));
            Require.NotEmpty(streamConsumerSessionsBlobContainerName, nameof(streamConsumerSessionsBlobContainerName));
            Require.NotEmpty(pendingNotificationsTableName, nameof(pendingNotificationsTableName));
            Require.NotEmpty(pendingNotificationsChaserExclusiveAccessLockBlobContainerName, nameof(pendingNotificationsChaserExclusiveAccessLockBlobContainerName));
            Require.NotEmpty(pendingNotificationsChaserExclusiveAccessLockBlobName, nameof(pendingNotificationsChaserExclusiveAccessLockBlobName));

            StorageConnectionString = storageConnectionString;
            JournalTableName = journalTableName;
            EventStoreDeploymentTableName = eventStoreDeploymentTableName;
            NotificationQueueName = notificationQueueName;
            NotificationQueuePartitionCount = notificationQueuePartitionCount;
            FailedNotificationsTableName = failedNotificationsTableName;
            StreamConsumerSessionsBlobContainerName = streamConsumerSessionsBlobContainerName;
            PendingNotificationsTableName = pendingNotificationsTableName;
            PendingNotificationsChaserExclusiveAccessLockBlobName = pendingNotificationsChaserExclusiveAccessLockBlobName;
            PendingNotificationsChaserExclusiveAccessLockBlobContainerName = pendingNotificationsChaserExclusiveAccessLockBlobContainerName;

            return this;
        }

        public IEventMutationPipelineConfiguration Mutate => m_mutationPipelineConfiguration;

        public INotificationProcessingConfiguration Notifications => m_notificationProcessingConfiguration;

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

        public string FailedNotificationsTableName
        {
            get;
            private set;
        }

        public string StreamConsumerSessionsBlobContainerName
        {
            get;
            private set;
        }

        public string PendingNotificationsTableName
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

        public bool BackgroundProcessingEnabled => m_notificationProcessingConfiguration.BackgroundProcessingEnabled;

        public IReadOnlyCollection<IEventMutator> IncomingMessageMutators => m_incomingMessageMutators;

        public IReadOnlyCollection<IEventMutator> OutgoingMessageMutators => m_outgoingMessageMutators;

        public IReadOnlyCollection<INotificationListener> NotificationListeners => m_notificationListeners;
    }
}
