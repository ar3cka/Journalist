using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Tables;
using Xunit;

namespace Journalist.WindowsAzure.Storage.IntegrationTests.Tables
{
    public class CloudTableTests
    {
        public CloudTableTests()
        {
            Factory = new StorageFactory();
        }

        [Fact]
        public async Task SegmentedRangeQueryTest()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            await InsertValues(table, partition);

            var query = table.PrepareEntityFilterSegmentedRangeQuery(
                "PartitionKey eq '{0}'".FormatString(partition),
                EmptyArray.Get<string>());

            var result = await query.ExecuteAsync();
            Assert.Equal(1000, result.Count);
            Assert.True(query.HasMore);

            result = await query.ExecuteAsync();
            Assert.Equal(1000, result.Count);
            Assert.False(query.HasMore);
        }

        private static async Task InsertValues(ICloudTable table, string partition)
        {
            foreach (var i in Enumerable.Range(1, 20))
            {
                var operation = table.PrepareBatchOperation();

                foreach (var j in Enumerable.Range(1, 100))
                {
                    operation.Insert(partition, i + ":" + j, EmptyDictionary.Get<string, object>());
                }

                await operation.ExecuteAsync();
            }
        }

        public StorageFactory Factory { get; set; }
    }
}
