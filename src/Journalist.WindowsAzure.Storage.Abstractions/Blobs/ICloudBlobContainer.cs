namespace Journalist.WindowsAzure.Storage.Blobs
{
    public interface ICloudBlobContainer
    {
        ICloudBlockBlob CreateBlockBlob(string resourceName);
    }
}
