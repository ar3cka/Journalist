using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence.Operations
{
    public abstract class JournalTableOperation
    {
        private readonly ICloudTable m_table;
        private readonly string m_streamName;

        private IBatchOperation m_operation;

        protected JournalTableOperation(ICloudTable table, string streamName)
        {
            Require.NotNull(table, "table");
            Require.NotEmpty(streamName, "steamName");

            m_table = table;
            m_streamName = streamName;
        }

        protected Task<IReadOnlyList<OperationResult>> ExecuteBatchOperationAsync()
        {
            AssertOperationPrepared();

            return m_operation.ExecuteAsync();
        }

        protected void PrepareBatchOperation()
        {
            m_operation = m_table.PrepareBatchOperation();
        }

        protected void Insert(string rowKey, IReadOnlyDictionary<string, object> properties)
        {
            AssertOperationPrepared();

            m_operation.Insert(m_streamName, rowKey, properties);
        }

        protected void Merge(string rowKey, string etag, IReadOnlyDictionary<string, object> properties)
        {
            AssertOperationPrepared();

            m_operation.Merge(m_streamName, rowKey, etag, properties);
        }

        private void AssertOperationPrepared()
        {
            Ensure.True(m_operation != null, "Operation was not prepared.");
        }

        public string StreamName
        {
            get { return m_streamName; }
        }
    }
}
