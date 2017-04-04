using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;
using Serilog;

namespace Journalist.EventStore.Notifications
{
    public class FailedNotificationsStore : IFailedNotificationsStore
    {
        private readonly ICloudTable m_failedNotificationsTable;
        private readonly INotificationFormatter m_notificationFormatter;
        private readonly IList<INotificationListener> m_notificationListeners = new List<INotificationListener>();

        private const string PAYLOAD_FIELD_NAME = "Payload";

        private static readonly ILogger s_logger = Log.ForContext<FailedNotificationsStore>();

        public FailedNotificationsStore(ICloudTable failedNotificationsTable, INotificationFormatter notificationFormatter)
        {
            Require.NotNull(failedNotificationsTable, nameof(failedNotificationsTable));
            Require.NotNull(notificationFormatter, nameof(notificationFormatter));

            m_failedNotificationsTable = failedNotificationsTable;
            m_notificationFormatter = notificationFormatter;
        }

        public Task PutToFailedAsync(INotification notification)
        {
            Require.NotNull(notification, nameof(notification));

            var batchOperation = m_failedNotificationsTable.PrepareBatchOperation();
            batchOperation.Insert(
                notification.NotificationType,
                notification.NotificationId.ToString(),
                new Dictionary<string, object> { { PAYLOAD_FIELD_NAME, m_notificationFormatter.ToBytes(notification) } });
            return batchOperation.ExecuteAsync();
        }

        public async Task<INotification[]> ReceiveFailedAsync()
        {
            var query = m_failedNotificationsTable.PrepareEntityRangeQueryByPartition(typeof(EventStreamUpdated).FullName);

            var result = await query.ExecuteAsync();

            if (result == null || result.IsEmpty())
            {
                return EmptyArray.Get<INotification>();
            }

            var failedNotifications = result
                .Where(row => row.ContainsKey(PAYLOAD_FIELD_NAME) && row[PAYLOAD_FIELD_NAME] is MemoryStream)
                .Select(row => EventStreamUpdated.RestoreFrom((MemoryStream)row[PAYLOAD_FIELD_NAME]));

            return failedNotifications.Cast<INotification>().ToArray();
        }

        public async Task<bool> RetryNotificationAsync(NotificationId notificationId)
        {
            Require.NotNull(notificationId, nameof(notificationId));

            try
            {
                var query = m_failedNotificationsTable.PrepareEntityPointQuery(
                    typeof(EventStreamUpdated).FullName,
                    notificationId.ToString());
                var result = await query.ExecuteAsync();
                if (result == null || !result.ContainsKey(PAYLOAD_FIELD_NAME) || !(result[PAYLOAD_FIELD_NAME] is MemoryStream))
                {
                    s_logger.Information("Failed notification not found: {NotificationId}", notificationId);
                    return true;
                }

                var notification = EventStreamUpdated.RestoreFrom((MemoryStream)result[PAYLOAD_FIELD_NAME]);
                foreach (var notificationListener in m_notificationListeners.Where(notification.IsAddressedTo))
                {
                    await notificationListener.On(notification);
                }

                await RemoveFromFailedAsync(notificationId.YieldArray());

                return true;
            }
            catch (Exception exception)
            {
                s_logger.Warning(exception, "Failed to retry notification {NotificationId}", notificationId);
                return false;
            }
        }

        public void AddRetryListener(INotificationListener notificationsListener)
        {
            Require.NotNull(notificationsListener, nameof(notificationsListener));

            m_notificationListeners.Add(notificationsListener);
        }

        public Task RemoveFromFailedAsync(NotificationId[] notificationIds)
        {
            Require.NotNull(notificationIds, nameof(notificationIds));
            
            var deleteOperations = new List<IBatchOperation>();
            var partitionKey = typeof(EventStreamUpdated).FullName;
            notificationIds.Aggregate(deleteOperations, (batchOperations, notificationId) =>
            {
                if (batchOperations.Last().IsMaximumOperationsCountWasReached)
                {
                    batchOperations.Add(m_failedNotificationsTable.PrepareBatchOperation());
                }

                batchOperations.Last().Delete(partitionKey, notificationId.ToString());
                return batchOperations;
            });

            return Task.WhenAll(deleteOperations.Select(operation => operation.ExecuteAsync()));
        }
    }
}