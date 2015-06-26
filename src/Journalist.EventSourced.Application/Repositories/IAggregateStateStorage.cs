using System.Threading.Tasks;
using Journalist.EventSourced.Entities;
using Journalist.Options;

namespace Journalist.EventSourced.Application.Repositories
{
    public interface IAggregateStateStorage<TState> where TState : IAggregateState
    {
        Task<Option<TState>> RestoreStateAsync(IIdentity identity);

        Task PersistAsync(IIdentity identity, TState state);
    }
}
