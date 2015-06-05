using System;
using Journalist.WindowsAzure.Storage.Tables;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Journalist.WindowsAzure.Storage
{
    public static class StorageFactory
    {
        public static ICloudTable CreateTable(string connectionString, string tableName)
        {
            Require.NotEmpty(connectionString, "connectionString");
            Require.NotEmpty(tableName, "tableName");

            return new CloudTableAdapter(
                CreateTableFactory(connectionString, tableName));
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
    }
}