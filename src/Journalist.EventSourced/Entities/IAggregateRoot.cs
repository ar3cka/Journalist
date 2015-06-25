namespace Journalist.EventSourced.Entities
{
    public interface IAggregateRoot<out TIdentity, out TState>
        where TIdentity : IIdentity
        where TState : IAggregateState
    {
        TIdentity Id { get; }

        TState State { get; }
    }
}
