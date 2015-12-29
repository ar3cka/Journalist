using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTableEntityQuery
    {
        Task<Dictionary<string, object>> ExecuteAsync();

        Dictionary<string, object> Execute();
    }
}
