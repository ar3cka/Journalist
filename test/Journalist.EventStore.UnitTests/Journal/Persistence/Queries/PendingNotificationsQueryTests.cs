using System;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Queries;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Queries
{
    public class PendingNotificationsQueryTests
    {
        private static readonly string s_queryTemplate = "RowKey ge '{0}{1}' and RowKey le '{0}{2}'".FormatString(
            EventJournalTableKeys.PendingNotificationPrefix,
            StreamVersion.Unknown,
            StreamVersion.Max);

        [Theory]
        [PendingNotificationsQueryTestData(Prepared = false)]
        public async Task ExecuteAsync_WhenPrepareHasNotBeenCalled_Throws(PendingNotificationsQuery query)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(query.ExecuteAsync);
        }

        [Theory]
        [PendingNotificationsQueryTestData(Prepared = false)]
        public void Prepare_CreatesSegmentedQuery(
            [Frozen] Mock<ICloudTable> tableMock,
            PendingNotificationsQuery query)
        {
            query.Prepare();

            tableMock.Verify(
                self => self.PrepareEntityFilterSegmentedRangeQuery(s_queryTemplate, EmptyArray.Get<string>()),
                Times.Once());
        }
    }
}