using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Types;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;
using Serilog;

namespace Journalist.EventStore.Notifications
{
    public class PendingNotifications : IPendingNotifications
    {
        private static readonly ILogger s_logger = Log.ForContext<PendingNotifications>();

        private readonly ICloudTable m_table;

        public PendingNotifications(ICloudTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public Task AddAsync(string streamName, StreamVersion streamVersion, int eventCount)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.Positive(eventCount, "eventCount");

            var operation = m_table.PrepareBatchOperation();
            var toVersion = streamVersion.Increment(eventCount);
            operation.InsertOrReplace(
                partitionKey: GetPartitionKey(streamName),
                rowKey: GetRowKey(streamName, streamVersion),
                propertyName: "ToVersion",
                propertyValue: (int)toVersion);

            return operation.ExecuteAsync();
        }

        public async Task DeleteAsync(string streamName, StreamVersion streamVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            try
            {
                var operation = m_table.PrepareBatchOperation();

                operation.Delete(
                    partitionKey: GetPartitionKey(streamName),
                    rowKey: GetRowKey(streamName, streamVersion),
                    etag: "*");

                await operation.ExecuteAsync();
            }
            catch (BatchOperationException exception)
            {
                if (exception.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                s_logger.Debug(exception, "Stream {SteamName} version {Version} pending notification record has been deleted.", streamName, streamVersion);
            }
        }

        public Task DeleteAsync(string streamName, StreamVersion[] streamVersions)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(streamVersions, "streamVersions");

            var operation = m_table.PrepareBatchOperation();

            foreach (var streamVersion in streamVersions)
            {
                operation.Delete(
                    partitionKey: GetPartitionKey(streamName),
                    rowKey: GetRowKey(streamName, streamVersion),
                    etag: "*");
            }

            return operation.ExecuteAsync();
        }

        public async Task<IDictionary<string, List<EventStreamUpdated>>> LoadAsync()
        {
            var result = new Dictionary<string, List<EventStreamUpdated>>();
            var query = m_table.PrepareEntityGetAllQuery();
            var queryResult = await query.ExecuteAsync();
            foreach (var row in queryResult)
            {
                var rowKey = (string)row[KnownProperties.RowKey];
                var rowKeyParts = rowKey.Split('|');
                var streamName = rowKeyParts[0];
                var fromVersion = StreamVersion.Parse(rowKeyParts[1]);
                var toVersion = StreamVersion.Create((int)row["ToVersion"]);

                List<EventStreamUpdated> streamNotifications;
                if (result.ContainsKey(streamName))
                {
                    streamNotifications = result[streamName];
                }
                else
                {
                    streamNotifications = new List<EventStreamUpdated>();
                    result[streamName] = streamNotifications;
                }

                streamNotifications.Add(new EventStreamUpdated(streamName, fromVersion, toVersion));
            }

            return result;
        }

        private static string GetPartitionKey(string streamName)
        {
            using (var hashAlg = MD5.Create())
            {
                var hash = BitConverter.ToInt16(hashAlg.ComputeHash(Encoding.UTF8.GetBytes(streamName)), 0);
                var partition = hash % 100;
                return partition.ToString("D3");
            }
        }

        private static string GetRowKey(string streamName, StreamVersion streamVersion)
        {
            return "{0}|{1}".FormatString(streamName, streamVersion.ToString());
        }
    }
}
