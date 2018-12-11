namespace Journalist.EventStore.Events.Mutation
{
    public interface IEventMutationPipelineFactory
    {
        IEventMutationPipeline CreateIncomingPipeline();

        IEventMutationPipeline CreateOutgoingPipeline();
    }
}
