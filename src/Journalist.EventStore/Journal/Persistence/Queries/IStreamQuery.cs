using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.Persistence.Queries
{
    public interface IStreamQuery<TResult>
    {
        void Prepare();

        Task<IReadOnlyList<TResult>> ExecuteAsync();
    }
}