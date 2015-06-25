using System.Threading.Tasks;
using Journalist.Options;

namespace Journalist.EventSourced.Entities
{
    public interface IAggregateRootRepository<TAggregateRoot, in TIdentity, TState>
        where TAggregateRoot : IAggregateRoot<TIdentity, TState>
        where TIdentity : IIdentity
        where TState : IAggregateState
    {
        Task<Option<TAggregateRoot>> TryLoadAsync(TIdentity id);

        Task<TAggregateRoot> LoadAsync(TIdentity id);

        Task SaveAsync(TAggregateRoot aggregateRoot);
    }
}