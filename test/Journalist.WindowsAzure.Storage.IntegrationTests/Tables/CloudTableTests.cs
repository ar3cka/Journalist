using System;
using System.Collections.Generic;
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
            Table = Factory.CreateTable("UseDevelopmentStorage=true", "T" + Guid.NewGuid().ToString("N"));
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
        public async Task SegmentedRangeQueryWithContinuationTokenTest()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            await InsertValues(table, partition);

            var query = table.PrepareEntityFilterSegmentedRangeQuery("PartitionKey eq '{0}'".FormatString(partition));
            var result = await query.ExecuteAsync();
            var continuationToken = query.ContinuationToken;
            Assert.Equal(1000, result.Count);
            Assert.True(query.HasMore);

            query = table.PrepareEntityFilterSegmentedRangeQuery("PartitionKey eq '{0}'".FormatString(partition));
            result = await query.ExecuteAsync(continuationToken);
            Assert.Equal(1000, result.Count);
            Assert.False(query.HasMore);
        }

        [Fact]
        public async Task GetAllSegmentedQueryTest()
        {
            var partition = Guid.NewGuid().ToString();

            var rowsCount = await InsertValues(Table, partition, 1);

            var query = Table.PrepareEntityGetAllSegmentedQuery();
            var result = await query.ExecuteAsync();

            Assert.Equal(rowsCount, result.Count);
        }

        [Fact]
        public async Task GetAllQueryTest()
        {
            var partition = Guid.NewGuid().ToString();

            var rowsCount = await InsertValues(Table, partition, 1);

            var query = Table.PrepareEntityGetAllQuery();
            var result = await query.ExecuteAsync();

            Assert.Equal(rowsCount, result.Count);
        }

        [Fact]
        public async Task GetAllSegmentedSyncQueryTest()
        {
            var partition = Guid.NewGuid().ToString();

            var rowsCount = await InsertValues(Table, partition, 1);

            var query = Table.PrepareEntityGetAllSegmentedQuery();
            var result = query.Execute();

            Assert.Equal(rowsCount, result.Count);
        }

        [Fact]
        public async Task GetAllSyncQueryTest()
        {
            var partition = Guid.NewGuid().ToString();

            var rowsCount = await InsertValues(Table, partition, 1);

            var query = Table.PrepareEntityGetAllQuery();
            var result = query.Execute();

            Assert.Equal(rowsCount, result.Count);
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
        public async Task Insert_WithPartitionKeyOnly_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition);

            Assert.NotNull(await query.ExecuteAsync());
        }

        [Fact]
        public async Task Delete_WithPartitionKeyOnly_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition);
            var result = await operation.ExecuteAsync();

            operation = table.PrepareBatchOperation();
            operation.Delete(partition, result[0].ETag);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition);
            Assert.Null(await query.ExecuteAsync());
        }

        [Fact]
        public async Task Merge_WithPartitionKeyOnly_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition);
            var result = await operation.ExecuteAsync();

            operation = table.PrepareBatchOperation();
            operation.Merge(partition, result[0].ETag);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition);
            Assert.NotNull(await query.ExecuteAsync());
        }

        [Fact]
        public async Task Replace_WithPartitionKeyOnly_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(partition);
            var result = await operation.ExecuteAsync();

            operation = table.PrepareBatchOperation();
            operation.Replace(partition, result[0].ETag);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition);
            Assert.NotNull(await query.ExecuteAsync());
        }

        [Fact]
        public async Task InsertOrMerge_WithPartitionKeyOnly_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.InsertOrMerge(partition);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition);
            Assert.NotNull(await query.ExecuteAsync());
        }

        [Fact]
        public async Task InsertOrReplace_WithPartitionKeyOnly_Test()
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.InsertOrReplace(partition);
            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition);
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

        [InlineData(1)]
        [InlineData(1L)]
        [InlineData("1")]
        [InlineData(true)]
        [Theory]
        public async Task PropertyTest<T>(T data)
        {
            var table = Factory.CreateTable("UseDevelopmentStorage=true", "TestCloudTable");
            var partition = Guid.NewGuid().ToString();
            var row = Guid.NewGuid().ToString();

            var operation = table.PrepareBatchOperation();
            operation.Insert(
                partition,
                row,
                new Dictionary<string, object>
                {
                    { "a", data }
                });

            await operation.ExecuteAsync();

            var query = table.PrepareEntityPointQuery(partition, row);
            var result = await query.ExecuteAsync();

            Assert.Equal(data, result["a"]);
        }

        private static async Task<int> InsertValues(ICloudTable table, string partition, int x = 20, int y = 100)
        {
            foreach (var i in Enumerable.Range(1, x))
            {
                var operation = table.PrepareBatchOperation();

                foreach (var j in Enumerable.Range(1, y))
                {
                    operation.Insert(partition, i + ":" + j, EmptyDictionary.Get<string, object>());
                }

                await operation.ExecuteAsync();
            }

            return x * y;
        }

        public StorageFactory Factory { get; set; }

        public ICloudTable Table { get; set; }
    }
}
