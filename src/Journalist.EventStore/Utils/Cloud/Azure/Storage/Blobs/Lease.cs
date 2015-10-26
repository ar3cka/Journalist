using System;
using System.Threading.Tasks;
using Journalist.Extensions;
using Journalist.WindowsAzure.Storage.Blobs;

namespace Journalist.EventStore.Utils.Cloud.Azure.Storage.Blobs
{
    public class Lease
    {
        public static readonly Lease NotAcquired = new Lease();

        private readonly string m_leaseId;

        private Lease(string leaseId)
        {
            m_leaseId = leaseId;
        }

        private Lease()
        {
        }

        public static async Task<Lease> AcquireAsync(ICloudBlockBlob blob, TimeSpan leasePeriod)
        {
            Require.NotNull(blob, "blob");

            try
            {
                var leaseId = await blob.AcquireLeaseAsync(leasePeriod);

                return new Lease(leaseId);

            }
            catch (LeaseAlreadyAcquiredException)
            {
            }

            return NotAcquired;
        }


        public static async Task<Lease> ReleaseAsync(ICloudBlockBlob blob, Lease lease)
        {
            Require.NotNull(blob, "blob");
            Require.NotNull(lease, "lease");

            Ensure.True(lease.m_leaseId.IsNotNullOrEmpty(), "Lease was not acquired");

            await blob.ReleaseLeaseAsync(lease.m_leaseId);

            return NotAcquired;
        }

        public static bool IsAcquired(Lease lease)
        {
            Require.NotNull(lease, "lease");

            return lease.m_leaseId.IsNotNullOrEmpty();
        }
    }
}
