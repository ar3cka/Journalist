using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTableEntitySegmentedRangeQuery
    {
        bool HasMore { get; }

        Task<List<Dictionary<string, object>>> ExecuteAsync();
    }
}
