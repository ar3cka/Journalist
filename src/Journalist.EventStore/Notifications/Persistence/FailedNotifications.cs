using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Tables;
using Serilog;

namespace Journalist.EventStore.Notifications.Persistence
{
    public sealed class FailedNotifications : IFailedNotifications
    {
        private static readonly ILogger s_logger = Log.ForContext<FailedNotifications>();

        private readonly ICloudTable m_table;

        public FailedNotifications(ICloudTable table)
        {
            Require.NotNull(table, nameof(table));

            m_table = table;
        }

        public async Task AddAsync(IFailedNotification failedNotification)
        {
            Require.NotNull(failedNotification, nameof(failedNotification));

            var operation = m_table.PrepareBatchOperation();
            operation.InsertOrReplace(
                partitionKey: GetPartitionKey(failedNotification.Id),
                properties: failedNotification.Properties.ToDictionary(pair => pair.Key, pair => (object)pair.Value));

            await operation.ExecuteAsync();
        }

        public async Task DeleteAsync(string failedNotificationId)
        {
            Require.NotEmpty(failedNotificationId, nameof(failedNotificationId));

            var operation = m_table.PrepareBatchOperation();
            
            operation.Delete(
                partitionKey: GetPartitionKey(failedNotificationId),
                etag: "*");
            
            try
            {
                await operation.ExecuteAsync();
            }
            catch (BatchOperationException exception) when (exception.HttpStatusCode == HttpStatusCode.NotFound)
            {
                s_logger.Debug(exception, "Failed notification is already deleted. {FailedNotificationId}.", failedNotificationId);
            }
        }

        private static string GetPartitionKey(string failedNotificationId)
        {
            return failedNotificationId;
        }
    }
}
