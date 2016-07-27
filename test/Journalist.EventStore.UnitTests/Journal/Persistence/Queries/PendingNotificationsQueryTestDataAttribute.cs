using System.Collections.Generic;
using System.Linq;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.Persistence;
using Journalist.EventStore.Journal.Persistence.Queries;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.WindowsAzure.Storage.Tables;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventStore.UnitTests.Journal.Persistence.Queries
{
    public class PendingNotificationsQueryTestDataAttribute : AutoMoqDataAttribute
    {
        public PendingNotificationsQueryTestDataAttribute()
        {
            Fixture.Customize(new StreamVersionCustomization());

            Fixture.Customize<List<Dictionary<string, object>>>(composer => composer
                .FromFactory((List<StreamVersion> versions) => versions
                    .Select(GeneratePendingNotificationRow)
                    .ToList()));

            Fixture.Customize<Mock<ICloudTable>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.PrepareEntityFilterSegmentedRangeQuery(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string[]>()))
                    .ReturnsUsingFixture(Fixture)));

            Fixture.Customize<PendingNotificationsQuery>(composer => composer
                .Do(PrepareQuery));
        }

        private Dictionary<string, object> GeneratePendingNotificationRow(StreamVersion version)
        {
            return new Dictionary<string, object>
            {
                [KnownProperties.PartitionKey] = Fixture.Create("stream"),
                [KnownProperties.RowKey] = EventJournalTableKeys.GetPendingNotificationRowKey(version),
                [EventJournalTableRowPropertyNames.Version] = version.ToString()
            };
        }

        private void PrepareQuery(PendingNotificationsQuery query)
        {
            if (Prepared)
            {
                query.Prepare();
            }
        }

        public bool Prepared { get; set; } = true;
    }
}