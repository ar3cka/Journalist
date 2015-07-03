namespace Journalist.EventStore.Events.Mutation
{
    public interface IEventMutator
    {
        JournaledEvent Mutate(JournaledEvent journaledEvent);
    }
}
