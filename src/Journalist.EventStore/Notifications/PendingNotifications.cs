using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications
{
    public class PendingNotifications : IPendingNotifications
    {
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

        public Task<IReadOnlyList<EventStreamUpdated>> LoadAsync()
        {
            throw new System.NotImplementedException();
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
