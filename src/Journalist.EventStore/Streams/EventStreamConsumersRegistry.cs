using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumersRegistry : IEventStreamConsumersRegistry
    {
        private readonly ICloudTable m_consumerMetadataTable;

        public EventStreamConsumersRegistry(ICloudTable consumerMetadataTable)
        {
            Require.NotNull(consumerMetadataTable, "consumerMetadataTable");

            m_consumerMetadataTable = consumerMetadataTable;
        }

        public async Task<EventStreamReaderId> RegisterAsync(string consumerName)
        {
            Require.NotEmpty(consumerName, "consumerName");

            try
            {
                var consumerId = EventStreamReaderId.Create();
                await InsertConsumerId(consumerName, consumerId);

                return consumerId;
            }
            catch (BatchOperationException exception)
            {
                if (exception.HttpStatusCode != HttpStatusCode.Conflict)
                {
                    throw;
                }
            }

            return await QueryConsumerId(consumerName);
        }

        public async Task<bool> IsResistedAsync(EventStreamReaderId consumerId)
        {
            Require.NotNull(consumerId, "consumerId");

            var consumerName = await QueryConsumerName(consumerId);

            return consumerName != null;
        }

        private async Task<EventStreamReaderId> QueryConsumerId(string consumerName)
        {
            var query = m_consumerMetadataTable.PrepareEntityPointQuery(
                partitionKey: Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
                rowKey: consumerName);

            var result = await query.ExecuteAsync();

            return EventStreamReaderId.Parse(
                (string)result[Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_CONSUMER_ID]);
        }

        private async Task<string> QueryConsumerName(EventStreamReaderId consumerId)
        {
            var query = m_consumerMetadataTable.PrepareEntityPointQuery(
                partitionKey: Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
                rowKey: consumerId.ToString());

            var result = await query.ExecuteAsync();
            if (result == null)
            {
                return null;
            }

            return (string)result[Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_CONSUMER_NAME];
        }

        private async Task InsertConsumerId(string consumerName, EventStreamReaderId consumerId)
        {
            var operation = m_consumerMetadataTable.PrepareBatchOperation();

            operation.Insert(
                partitionKey: Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
                rowKey: consumerName,
                properties: new Dictionary<string, object>
                {
                    { Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_CONSUMER_ID, consumerId.ToString() }
                });

            operation.Insert(
                partitionKey: Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
                rowKey: consumerId.ToString(),
                properties: new Dictionary<string, object>
                {
                    {
                        Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_CONSUMER_NAME, consumerName
                    }
                });

            await operation.ExecuteAsync();
        }
    }
}
