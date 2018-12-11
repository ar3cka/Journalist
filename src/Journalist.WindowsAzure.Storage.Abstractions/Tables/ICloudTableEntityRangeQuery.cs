using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTableEntityRangeQuery
    {
        Task<List<Dictionary<string, object>>> ExecuteAsync();

        List<Dictionary<string, object>> Execute();
    }
}
