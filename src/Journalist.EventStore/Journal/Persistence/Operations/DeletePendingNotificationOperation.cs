using System.Net;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Utils;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.EventStore.Journal.Persistence.Operations
{
    public class DeletePendingNotificationOperation : JournalTableOperation<Nothing>
    {
        public DeletePendingNotificationOperation(ICloudTable table, string streamName) : base(table, streamName)
        {
        }

        public void Prepare(StreamVersion version)
        {
            PrepareBatchOperation();

            Delete(EventJournalTableKeys.PendingNotificationPrefix + version);
        }

        public void Prepare(StreamVersion[] versions)
        {
            PrepareBatchOperation();

            foreach (var streamVersion in versions)
            {
                Delete(EventJournalTableKeys.PendingNotificationPrefix + streamVersion);
            }
        }

        public async override Task<Nothing> ExecuteAsync()
        {
            try
            {
                await ExecuteBatchOperationAsync();
            }
            catch (BatchOperationException exception)
            {
                if (exception.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            return Nothing.Value;
        }
    }
}
