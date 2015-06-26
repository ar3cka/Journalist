using System.Threading.Tasks;
using Journalist.EventSourced.Entities;
using Journalist.Options;

namespace Journalist.EventSourced.Application.Repositories
{
    public abstract class AbstractAggregateRootRepository<TAggregateRoot, TIdentity, TState> :
        IAggregateRootRepository<TAggregateRoot, TIdentity, TState>
        where TAggregateRoot : IAggregateRoot<TIdentity, TState>
        where TState : IAggregateState
        where TIdentity : IIdentity
    {
        private readonly IAggregateStateStorage<TState> m_stateStorage;

        protected AbstractAggregateRootRepository(IAggregateStateStorage<TState> stateStorage)
        {
            Require.NotNull(stateStorage, "stateStorage");

            m_stateStorage = stateStorage;
        }

        public async Task<Option<TAggregateRoot>> TryLoadAsync(TIdentity id)
        {
            Require.NotNull(id, "id");

            var state = await m_stateStorage.RestoreStateAsync(id);

            return state.Select(s => RestoreAggregateRoot(id, s));
        }

        public async Task<TAggregateRoot> LoadAsync(TIdentity id)
        {
            Require.NotNull(id, "id");

            var state = await m_stateStorage.RestoreStateAsync(id);

            return state.Match(
                s => RestoreAggregateRoot(id, s),
                () => StateNotFound(id));
        }

        public async  Task SaveAsync(TAggregateRoot aggregateRoot)
        {
            Require.NotNull(aggregateRoot, "aggregateRoot");

            await m_stateStorage.PersistAsync(aggregateRoot.Id, aggregateRoot.State);
        }

        protected abstract TAggregateRoot RestoreAggregateRoot(TIdentity identity, TState state);

        protected abstract TAggregateRoot StateNotFound(TIdentity identity);
    }
}
