using System;
using Journalist.WindowsAzure.Storage.Blobs;
using Journalist.WindowsAzure.Storage.Tables;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage
{
    public class StorageFactory : IStorageFactory
    {
        public ICloudTable CreateTable(string connectionString, string tableName)
        {
            Require.NotEmpty(connectionString, "connectionString");
            Require.NotEmpty(tableName, "tableName");

            return new CloudTableAdapter(CreateTableFactory(connectionString, tableName));
        }

        public ICloudBlobContainer CreateBlobContainer(string connectionString, string containerName)
        {
            Require.NotEmpty(connectionString, "connectionString");
            Require.NotEmpty(containerName, "containerName");

            return new CloudBlobContainerAdapter(CreateBlobContainerFactory(connectionString, containerName));
        }

        private static Func<CloudTable> CreateTableFactory(string connectionString, string tableName)
        {
            return () =>
            {
                var account = CloudStorageAccount.Parse(connectionString);

                var tableClient = account.CreateCloudTableClient();
                var table = tableClient.GetTableReference(tableName);
                table.CreateIfNotExists();

                return table;
            };
        }

        private static Func<CloudBlobContainer> CreateBlobContainerFactory(string connectionString, string containerName)
        {
            return () =>
            {
                var account = CloudStorageAccount.Parse(connectionString);

                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                container.CreateIfNotExists();

                return container;
            };
        }
    }
}
