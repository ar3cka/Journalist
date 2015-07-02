namespace Journalist.EventStore.Events.Mutation
{
    public class EventStreamMutationPipeline : IEventMutationPipeline
    {
        public JournaledEvent Mutate(JournaledEvent journaledEvent)
        {
            Require.NotNull(journaledEvent, "journaledEvent");

            return journaledEvent;
        }
    }
}