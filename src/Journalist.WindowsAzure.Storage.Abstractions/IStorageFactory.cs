using Journalist.WindowsAzure.Storage.Blobs;
using Journalist.WindowsAzure.Storage.Tables;

namespace Journalist.WindowsAzure.Storage
{
    public interface IStorageFactory
    {
        ICloudTable CreateTable(string connectionString, string tableName);

        ICloudBlobContainer CreateBlobContainer(string connectionString, string containerName);
    }
}
