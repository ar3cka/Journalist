using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.WindowsAzure.Storage.Tables;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Operations
{
    public class AppendOperationTests : OperationFixture
    {
        [Theory]
        [AppendOperationTestsData]
        public void Prepare_CreatesBatchOperation(
            [Frozen] Mock<ICloudTable> tableMock,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            operation.Prepare(events);

            tableMock.Verify(self => self.PrepareBatchOperation(), Times.Once());
        }

        [Theory]
        [AppendOperationTestsData]
        public void Prepare_ForInitializedHeader_UpdatesHeaderRow(
            [Frozen] Mock<IBatchOperation> operationMock,
            [Frozen] string streamName,
            [Frozen] EventStreamHeader header,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            var targetVersion = (int)header.Version.Increment(events.Count());

            operation.Prepare(events);

            VerifyMergeOperation(
                operationMock, streamName,
                "HEAD",
                header.ETag,
                name => name.Equals("Version"),
                value => value.Equals(targetVersion));
        }

        [Theory]
        [AppendOperationTestsData(IsNewStream = true)]
        public void Prepare_ForUnknownHeader_InsertsHeaderRow(
            [Frozen] Mock<IBatchOperation> operationMock,
            [Frozen] string streamName,
            [Frozen] EventStreamHeader header,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            var targetVersion = (int)header.Version.Increment(events.Count());

            operation.Prepare(events);

            VerifyInsertOperation(
                operationMock,
                streamName,
                "HEAD",
                name => name.Equals("Version"),
                value => value.Equals(targetVersion));
        }

        [Theory]
        [AppendOperationTestsData]
        public void Prepare_InsertsPendingNotificationRow(
            [Frozen] Mock<IBatchOperation> operationMock,
            [Frozen] string streamName,
            [Frozen] EventStreamHeader header,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            var targetVersion = (int)header.Version.Increment(events.Count());

            operation.Prepare(events);

            VerifyInsertOperation(
                operationMock,
                streamName,
                "PNDNTF|" + header.Version,
                name => name.Equals("Version"),
                value => value.Equals(targetVersion));
        }

        [Theory]
        [AppendOperationTestsData]
        public void Prepare_InsertsEvents(
            [Frozen] Mock<IBatchOperation> operationMock,
            [Frozen] string streamName,
            [Frozen] EventStreamHeader header,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            operation.Prepare(events);

            var version = header.Version;
            foreach (var journaledEvent in events)
            {
                var e = journaledEvent;
                version = version.Increment();

                VerifyInsertOperation(
                    operationMock: operationMock,
                    partitionKey: streamName,
                    rowKey: version.ToString(),
                    verifyColumns: columns =>
                        columns["EventId"].Equals(e.EventId) &&
                        columns["EventType"].Equals(e.EventTypeName));
            }
        }

        [Theory]
        [AppendOperationTestsData]
        public async Task Execute_WhenOperationHasNotBeenPrepared_Throws(
            AppendOperation operation)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(operation.ExecuteAsync);
        }

        [Theory]
        [AppendOperationTestsData]
        public async Task Execute_ReturnsHeaderWithNewETag(
            [Frozen] IReadOnlyList<OperationResult> batchResult,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            operation.Prepare(events);

            var result = await operation.ExecuteAsync();

            Assert.Equal(batchResult[0].ETag, result.ETag);
        }

        [Theory]
        [AppendOperationTestsData]
        public async Task Execute_ReturnsHeaderWithIncrementedVersion(
            [Frozen] StreamVersion currentVersion,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            var targetVersion = currentVersion.Increment(events.Count());

            operation.Prepare(events);

            var result = await operation.ExecuteAsync();

            Assert.Equal(targetVersion, result.Version);
        }
    }
}
