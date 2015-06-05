using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.WindowsAzure.Storage.Tables
{
    public interface ICloudTableEntityQuery
    {
        Task<IDictionary<string, object>> ExecuteAsync();
    }
}