using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal
{
    public class EventJournalReaders : IEventJournalReaders
    {
        private readonly ICloudTable m_table;

        public EventJournalReaders(ICloudTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public Task RegisterAsync(EventStreamReaderId readerId)
        {
            Require.NotNull(readerId, "readerId");

            var operation = m_table.PrepareBatchOperation();
            operation.Insert(
                Constants.StorageEntities.MetadataTable.EVENT_STREAM_READERS_IDS_PK,
                readerId.ToString());

            return operation.ExecuteAsync();
        }

        public async Task<bool> IsRegisteredAsync(EventStreamReaderId readerId)
        {
            Require.NotNull(readerId, "readerId");

            var query = m_table.PrepareEntityPointQuery(
                Constants.StorageEntities.MetadataTable.EVENT_STREAM_READERS_IDS_PK,
                readerId.ToString());

            var result = await query.ExecuteAsync();

            return result != null;
        }
    }
}
