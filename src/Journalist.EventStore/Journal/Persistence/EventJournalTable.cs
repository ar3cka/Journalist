using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence
{
    public class EventJournalTable : IEventJournalTable
    {
        private static class Properties
        {
            public static readonly string[] ReferenceRowHead = EventJournalTableRowPropertyNames.Version.YieldArray();
        }

        private readonly ICloudTable m_table;

        public EventJournalTable(ICloudTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public AppendOperation CreateAppendOperation(string streamName, EventStreamHeader header)
        {
            return new AppendOperation(m_table, streamName, header);
        }

        public Task<IDictionary<string, object>> ReadStreamHeadPropertiesAsync(string streamName)
        {
            return ReadReferenceRowHeadAsync(streamName, "HEAD");
        }

        public Task<IDictionary<string, object>> ReadStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId)
        {
            return ReadReferenceRowHeadAsync(streamName, "RDR|" + readerId);
        }
		
		public async Task<IEnumerable<IDictionary<string, object>>> ReadAllStreamReadersPropertiesAsync(string streamName)
		{
			Require.NotEmpty(streamName, nameof(streamName));

			var query = m_table.PrepareEntityRangeQueryByRows(streamName, "RDR", "RDS", Properties.ReferenceRowHead);

			var headProperties = await query.ExecuteAsync();

			return headProperties ?? Enumerable.Empty<IDictionary<string, object>>();
		}

		public async Task InsertStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version)
        {
            var operation = m_table.PrepareBatchOperation();

            operation.Insert(
                streamName,
                "RDR|" + readerId,
                EventJournalTableRowPropertyNames.Version,
                (int)version);

            await operation.ExecuteAsync();
        }

        public async Task UpdateStreamReaderPropertiesAsync(string streamName, EventStreamReaderId readerId, StreamVersion version, string etag)
        {
            var operation = m_table.PrepareBatchOperation();

            operation.Merge(
                streamName,
                "RDR|" + readerId,
                etag,
                EventJournalTableRowPropertyNames.Version,
                (int)version);

            await operation.ExecuteAsync();
        }

        public async Task<FetchEventsResult> FetchStreamEvents(string stream, EventStreamHeader header, StreamVersion fromVersion, int sliceSize)
        {
            // fromVersion already in slice
            var isFetchingCompleted = false;
            var nextSliceVersion = fromVersion.Increment(sliceSize - 1);
            if (nextSliceVersion >= header.Version)
            {
                nextSliceVersion = header.Version;
                isFetchingCompleted = true;
            }

            const string queryTemplate =
                "((PartitionKey eq '{0}') and (RowKey eq 'HEAD')) or " +
                "((PartitionKey eq '{0}') and (RowKey ge '{1}' and RowKey le '{2}'))";

            var query = m_table.PrepareEntityFilterRangeQuery(
                queryTemplate.FormatString(
                    stream,
                    fromVersion.ToString(),
                    nextSliceVersion.ToString()));

            var queryResult = await query.ExecuteAsync();

            var events = new SortedList<StreamVersion, JournaledEvent>(sliceSize);
            foreach (var properties in queryResult)
            {
                var rowKey = (string)properties[KnownProperties.RowKey];
                if (!rowKey.EqualsCi("HEAD"))
                {
                    events.Add(StreamVersion.Parse((string)properties[KnownProperties.RowKey]), JournaledEvent.Create(properties));
                }
            }

            return new FetchEventsResult(isFetchingCompleted, events);
        }

        private async Task<IDictionary<string, object>> ReadReferenceRowHeadAsync(string streamName, string referenceType)
        {
            var query = m_table.PrepareEntityPointQuery(
                streamName,
                referenceType,
                Properties.ReferenceRowHead);

            var headProperties = await query.ExecuteAsync();

            return headProperties;
        }
    }
}
