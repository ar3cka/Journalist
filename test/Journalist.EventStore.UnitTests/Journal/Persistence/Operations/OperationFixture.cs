using System;
using System.Collections.Generic;
using Journalist.WindowsAzure.Storage.Tables;
using Moq;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Operations
{
    public class OperationFixture
    {
        protected static void VerifyInsertOperation(
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

        protected static void VerifyMergeOperation(
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

        protected static void VerifyDeleteOperation(
            Mock<IBatchOperation> operationMock,
            string partitionKey,
            string rowKey,
            string etag)
        {
            operationMock.Verify(
                self => self.Delete(
                    partitionKey,
                    rowKey,
                    etag));
        }
    }
}
