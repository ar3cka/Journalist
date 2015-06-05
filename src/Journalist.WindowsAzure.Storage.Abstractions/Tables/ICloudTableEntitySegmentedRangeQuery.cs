using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTableEntitySegmentedRangeQuery
    {
        bool HasMore { get; }

        Task<IList<IDictionary<string, object>>> ExecuteAsync();
    }
}
