using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.Notifications.Types;

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

        public async Task<IReadOnlyList<EventStreamUpdated>> LoadAsync()
        {
            var query = m_table.CreatePendingNotificationsQuery();
            query.Prepare();

            var knownStreams = new HashSet<string>();
            var result = new List<EventStreamUpdated>();
            IReadOnlyList<EventStreamUpdated> notifications;
            do
            {
                notifications = await query.ExecuteAsync();
                foreach (var notification in notifications)
                {
                    if (knownStreams.Add(notification.StreamName))
                    {
                        result.Add(notification);
                    }
                }
            }
            while (notifications.Any() || result.Count < MAX_NOTIFICATIONS);

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
