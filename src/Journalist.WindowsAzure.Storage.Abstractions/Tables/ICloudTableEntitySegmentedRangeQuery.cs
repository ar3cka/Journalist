using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTableEntitySegmentedRangeQuery
    {
        Task<List<Dictionary<string, object>>> ExecuteAsync();

        Task<List<Dictionary<string, object>>> ExecuteAsync(byte[] continuationToken);

        bool HasMore { get; }

        byte[] ContinuationToken { get; }
    }
}
