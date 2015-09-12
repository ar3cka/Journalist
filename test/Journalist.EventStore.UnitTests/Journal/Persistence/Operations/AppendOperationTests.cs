using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.Persistence.Operations;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Operations
{
    public class AppendOperationTests
    {
        [Theory]
        [AutoMoqData]
        public void Prepare_CreatesBatchOperation(
            [Frozen] Mock<ICloudTable> tableMock,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            operation.Prepare(events);

            tableMock
                .Verify(self => self.PrepareBatchOperation(), Times.Once());
        }

        [Theory]
        [AutoMoqData]
        public void Prepare_UpdatesHeaderRow(
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
                columns => columns["Version"].Equals(targetVersion));
        }

        [Theory]
        [AutoMoqData]
        public void Prepare_InsertsPendingNotificationRow(
            [Frozen] Mock<IBatchOperation> operationMock,
            [Frozen] string streamName,
            [Frozen] EventStreamHeader header,
            JournaledEvent[] events,
            AppendOperation operation)
        {
            var targetVersion = header.Version.Increment(events.Count());

            operation.Prepare(events);

            VerifyInsertOperation(
                operationMock,
                streamName,
                "PNDNTF|" + targetVersion,
                columns => columns.IsEmpty());
        }

        [Theory]
        [AutoMoqData]
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
                    operationMock,
                    streamName,
                    version.ToString(),
                    columns => columns["EventId"].Equals(e.EventId));
            }
        }

        [Theory]
        [AutoMoqData]
        public async Task Execute_WhenOperationHasNotBeenPrepared_Throws(
            AppendOperation operation)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(operation.ExecuteAsync);
        }

        [Theory]
        [AutoMoqData]
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
        [AutoMoqData]
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

        private static void VerifyInsertOperation(
            Mock<IBatchOperation> operationMock,
            string partitionKey,
            string rowKey,
            Func<IReadOnlyDictionary<string, object>, bool> verifyColumns)
        {
            operationMock.Verify(
                self => self.Insert(
                    partitionKey,
                    rowKey,
                    It.Is<IReadOnlyDictionary<string, object>>(columns => verifyColumns(columns))));
        }

        private static void VerifyMergeOperation(
            Mock<IBatchOperation> operationMock,
            string partitionKey,
            string rowKey,
            string etag,
            Func<IReadOnlyDictionary<string, object>, bool> verifyColumns)
        {
            operationMock.Verify(
                self => self.Merge(
                    partitionKey,
                    rowKey,
                    etag,
                    It.Is<IReadOnlyDictionary<string, object>>(columns => verifyColumns(columns))));
        }
    }
}
