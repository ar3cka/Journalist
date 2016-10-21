using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Blobs
{
    public interface ICloudBlockBlob
    {
        Task<string> AcquireLeaseAsync(TimeSpan? period, string proposedLeaseId = null);

        Task ReleaseLeaseAsync(string leaseId);

        Task RenewLeaseAsync(string leaseId);

        Task BreakLeaseAsync(TimeSpan? breakPeriod);

        Task<bool> IsExistsAsync();

        Task FetchAttributesAsync();

        Task SaveMetadataAsync();

        Task SaveMetadataAsync(string leaseId);

        Task UploadAsync(Stream stream);

        Task<Stream> DownloadAsync();

        Task DeleteAsync();

        Task<bool> IsLeaseLocked();

        IDictionary<string, string> Metadata { get; }
    }
}
