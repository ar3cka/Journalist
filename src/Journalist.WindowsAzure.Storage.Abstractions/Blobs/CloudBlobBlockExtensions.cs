using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Blobs
{
    public static class CloudBlobBlockExtensions
    {
        public static Task<string> AcquireLeaseAsync(this ICloudBlockBlob blob)
        {
            Require.NotNull(blob, "blob");

            return blob.AcquireLeaseAsync(null);
        }

        public static Task BreakLeaseAsync(this ICloudBlockBlob blob)
        {
            Require.NotNull(blob, "blob");

            return blob.BreakLeaseAsync(null);
        }
    }
}
