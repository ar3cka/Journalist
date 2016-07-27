using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications
{
    public class PendingNotifications : IPendingNotifications
    {
        private const int MAX_NOTIFICATIONS = 100;
        private readonly IEventJournalTable m_table;

        public PendingNotifications(IEventJournalTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public Task DeleteAsync(string streamName, StreamVersion streamVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            var operation = m_table.CreateDeletePendingNotificationOperation(streamName);
            operation.Prepare(streamVersion);

            return ExecuteOperationAsync(operation);
        }

        public Task DeleteAsync(string streamName, StreamVersion[] streamVersions)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(streamVersions, "streamVersions");

            var operation = m_table.CreateDeletePendingNotificationOperation(streamName);
            operation.Prepare(streamVersions);

            return ExecuteOperationAsync(operation);
        }

        public async Task<IDictionary<string, List<EventStreamUpdated>>> LoadAsync()
        {
            var query = m_table.CreatePendingNotificationsQuery();
            query.Prepare();

            var result = new Dictionary<string, List<EventStreamUpdated>>();
            var notifications = await query.ExecuteAsync();
            foreach (var notification in notifications)
            {
                List<EventStreamUpdated> streamNotifications;
                if (result.ContainsKey(notification.StreamName))
                {
                    streamNotifications = result[notification.StreamName];
                }
                else
                {
                    streamNotifications = new List<EventStreamUpdated>();
                    result[notification.StreamName] = streamNotifications;
                }

                streamNotifications.Add(notification);
            }

            return result;
        }

        private static async Task<TResult> ExecuteOperationAsync<TResult>(IStreamOperation<TResult> operation)
        {
            try
            {
                return await operation.ExecuteAsync();
            }
            catch (Exception exception)
            {
                operation.Handle(exception);

                throw;
            }
        }
    }
}
