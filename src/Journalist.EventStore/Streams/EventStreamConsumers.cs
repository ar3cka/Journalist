using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumers : IEventStreamConsumers
    {
        private readonly ConcurrentDictionary<string, EventStreamReaderId> m_cache = new ConcurrentDictionary<string, EventStreamReaderId>();
        private readonly ICloudTable m_consumerMetadataTable;

        public EventStreamConsumers(ICloudTable consumerMetadataTable)
        {
            Require.NotNull(consumerMetadataTable, "consumerMetadataTable");

            m_consumerMetadataTable = consumerMetadataTable;
        }

        public async Task<EventStreamReaderId> RegisterAsync(string consumerName)
        {
            Require.NotEmpty(consumerName, "consumerName");

            EventStreamReaderId consumerId;
            if (m_cache.TryGetValue(consumerName, out consumerId))
            {
                return consumerId;
            }

            var alreadyInserted = false;
            try
            {
                consumerId = EventStreamReaderId.Create();
                await InsertConsumerId(consumerName, consumerId);
            }
            catch (BatchOperationException exception)
            {
                if (exception.HttpStatusCode != HttpStatusCode.Conflict)
                {
                    throw;
                }

                alreadyInserted = true;
            }

            if (alreadyInserted)
            {
                consumerId = await QueryConsumerId(consumerName);
            }

            m_cache.TryAdd(consumerName, consumerId);

            return consumerId;
        }

	    public async Task<string> GetNameAsync(EventStreamReaderId eventStreamReaderId)
	    {
			 Require.NotNull(eventStreamReaderId, nameof(eventStreamReaderId));

		    var query = m_consumerMetadataTable.PrepareEntityPointQuery(
			    Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
			    eventStreamReaderId.ToString());

		    var result = await query.ExecuteAsync();

		    return result[Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_CONSUMER_NAME].ToString();
	    }

        private async Task<EventStreamReaderId> QueryConsumerId(string consumerName)
        {
            var query = m_consumerMetadataTable.PrepareEntityPointQuery(
                partitionKey: Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
                rowKey: consumerName);

            var result = await query.ExecuteAsync();

            return EventStreamReaderId.Parse(
                (string)result[Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_READER_ID]);
        }

        private async Task InsertConsumerId(string consumerName, EventStreamReaderId consumerId)
        {
            var operation = m_consumerMetadataTable.PrepareBatchOperation();

            operation.Insert(
                partitionKey: Constants.StorageEntities.MetadataTable.EVENT_STREAM_CONSUMERS_IDS_PK,
                rowKey: consumerName,
                properties: new Dictionary<string, object>
                {
                    { Constants.StorageEntities.MetadataTableProperties.EVENT_STREAM_READER_ID, consumerId.ToString() }
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
