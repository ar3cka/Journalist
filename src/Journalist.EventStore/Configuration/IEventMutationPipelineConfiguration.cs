using Journalist.EventStore.Events.Mutation;

namespace Journalist.EventStore.Configuration
{
    public interface IEventMutationPipelineConfiguration
    {
        IEventStoreConnectionConfiguration IncomingEventsWith<TEventMutator>(TEventMutator mutator)
            where TEventMutator : IEventMutator;

        IEventStoreConnectionConfiguration OutgoingEventsWith<TEventMutator>(TEventMutator mutator)
            where TEventMutator : IEventMutator;
    }
}
