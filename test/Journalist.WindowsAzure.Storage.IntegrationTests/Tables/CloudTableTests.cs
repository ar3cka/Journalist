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

            var query = table.PrepareEntityFilterSegmentedRangeQuery("PartitionKey eq '{0}'".FormatString(partition));

            var result = await query.ExecuteAsync();
            Assert.Equal(1000, result.Count);
            Assert.True(query.HasMore);

            result = await query.ExecuteAsync();
            Assert.Equal(1000, result.Count);
            Assert.False(query.HasMore);
        }

        [Fact]
        public async Task PrepareEntityRangeQueryByPartition_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition, "1");
            operation.Insert(partition, "2");
            operation.Insert(partition, "3");
            await operation.ExecuteAsync();

            var partitionQuery = table.PrepareEntityRangeQueryByPartition(partition);
            var results = await partitionQuery.ExecuteAsync();

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task PrepareEntityRangeQueryByRows_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition, "1");
            operation.Insert(partition, "2");
            operation.Insert(partition, "3");
            await operation.ExecuteAsync();

            var partitionQuery = table.PrepareEntityRangeQueryByRows(partition, "1", "3");
            var results = await partitionQuery.ExecuteAsync();

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task Insert_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();
            var row = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition, row);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition, row);

            Assert.NotNull(await query.ExecuteAsync());
        }

        [Fact]
        public async Task Delete_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();
            var row =  Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition, row);
            var result = await operation.ExecuteAsync();

            operation = table.PrepareBatchOperation();
            operation.Delete(partition, row, result[0].ETag);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition, row);

            Assert.Null(await query.ExecuteAsync());
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
