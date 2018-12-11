namespace Journalist.EventStore.Events.Mutation
{
    public interface IEventMutationPipeline
    {
        JournaledEvent Mutate(JournaledEvent journaledEvent);
    }
}