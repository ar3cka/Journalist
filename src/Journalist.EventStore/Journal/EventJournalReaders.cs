using System.Collections.Concurrent;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal
{
    public class EventJournalReaders : IEventJournalReaders
    {
        private readonly ConcurrentDictionary<EventStreamReaderId, bool> m_cache = new ConcurrentDictionary<EventStreamReaderId, bool>();
        private readonly ICloudTable m_table;

        public EventJournalReaders(ICloudTable table)
        {
            Require.NotNull(table, "table");

            m_table = table;
        }

        public async Task RegisterAsync(EventStreamReaderId readerId)
        {
            Require.NotNull(readerId, "readerId");

            var operation = m_table.PrepareBatchOperation();
            operation.Insert(
                Constants.StorageEntities.MetadataTable.EVENT_STREAM_READERS_IDS_PK,
                readerId.ToString());

            await operation.ExecuteAsync();

            m_cache.TryAdd(readerId, true);
        }

        public async Task<bool> IsRegisteredAsync(EventStreamReaderId readerId)
        {
            Require.NotNull(readerId, "readerId");

            bool exists;
            if (m_cache.TryGetValue(readerId, out exists) && exists)
            {
                return true;
            }

            var query = m_table.PrepareEntityPointQuery(
                Constants.StorageEntities.MetadataTable.EVENT_STREAM_READERS_IDS_PK,
                readerId.ToString());

            var result = await query.ExecuteAsync();
            exists = result != null;
            if (exists)
            {
                m_cache.TryAdd(readerId, true);
            }

            return exists;
        }
    }
}
