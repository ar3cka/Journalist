using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.WindowsAzure.Storage.Tables;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Operations
{
    public class DeletePendingNotificationOperationTest : OperationFixture
    {
        [Theory]
        [AutoMoqData]
        public void Prepare_DeletesPendingNotificationRow(
            [Frozen] Mock<IBatchOperation> operationMock,
            StreamVersion version,
            DeletePendingNotificationOperation operation)
        {
            operation.Prepare(version);

            VerifyDeleteOperation(
                operationMock,
                operation.StreamName,
                EventJournalTableKeys.PendingNotificationPrefix + version,
                "*");
        }
    }
}
