namespace Journalist.EventSourced.Entities
{
    public interface IAggregateRoot<out TIdentity, out TState>
        where TIdentity : IIdentity
        where TState : IAggregatePersistenceState
    {
        TIdentity Id { get; }

        TState State { get; }
    }
}
