using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Journalist.WindowsAzure.Storage.Blobs
{
    public class CloudBlockBlobAdapter : ICloudBlockBlob
    {
        private readonly CloudBlockBlob m_blob;

        public CloudBlockBlobAdapter(CloudBlockBlob blob)
        {
            Require.NotNull(blob, "blob");

            m_blob = blob;
        }

        public async Task<string> AcquireLeaseAsync(TimeSpan? period)
        {
            await EnsureBlobExistsAsync();

            try
            {
                return await m_blob.AcquireLeaseAsync(period, null);
            }
            catch (StorageException exception)
            {
                var blobLeased =
                    exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict
                    &&
                    exception.RequestInformation.ExtendedErrorInformation.ErrorCode ==
                    BlobErrorCodeStrings.LeaseAlreadyPresent;

                if (blobLeased)
                {
                    throw new LeaseAlreadyAcquiredException(
                        "Blob lease '{0}' already acquired.".FormatString(m_blob.Uri),
                        exception);
                }

                throw;
            }
        }

        public async Task ReleaseLeaseAsync(string leaseId)
        {
            Require.NotEmpty(leaseId, "leaseId");

            await m_blob.ReleaseLeaseAsync(new AccessCondition {LeaseId = leaseId});
        }

        public async Task RenewLeaseAsync(string leaseId)
        {
            Require.NotEmpty(leaseId, "leaseId");

            await m_blob.RenewLeaseAsync(new AccessCondition {LeaseId = leaseId});
        }

        public Task<bool> IsExistsAsync()
        {
            return m_blob.ExistsAsync();
        }

        public Task FetchAttributesAsync()
        {
            return m_blob.FetchAttributesAsync();
        }

        public Task SaveMetadataAsync()
        {
            return m_blob.SetMetadataAsync();
        }

        public Task UploadAsync(Stream stream)
        {
            Require.NotNull(stream, "stream");

            return m_blob.UploadFromStreamAsync(stream);
        }

        public async Task<Stream> DownloadAsync()
        {
            using (var stream = new MemoryStream())
            {
                await m_blob.DownloadToStreamAsync(stream);
                return new MemoryStream(stream.GetBuffer(), 0, (int) stream.Length, false);
            }
        }

        public Task DeleteAsync()
        {
            return m_blob.DeleteAsync();
        }

        public async Task<bool> IsLeaseLocked()
        {
            await m_blob.FetchAttributesAsync();

            return m_blob.Properties.LeaseStatus == LeaseStatus.Locked;
        }

        public IDictionary<string, string> Metadata
        {
            get { return m_blob.Metadata; }
        }

        private async Task EnsureBlobExistsAsync()
        {
            if (await m_blob.ExistsAsync())
            {
                return;
            }

            try
            {
                await m_blob.UploadFromByteArrayAsync(
                    EmptyArray.Get<byte>(),
                    0,
                    0,
                    AccessCondition.GenerateIfNoneMatchCondition("*"),
                    null,
                    null);
            }
            catch (StorageException exception)
            {
                // 412 from trying to modify a blob that's leased
                var blobLeased = exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed;

                var blobExists =
                    exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict &&
                    exception.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.BlobAlreadyExists;

                if (!blobExists && !blobLeased)
                {
                    throw;
                }
            }
        }
    }
}
