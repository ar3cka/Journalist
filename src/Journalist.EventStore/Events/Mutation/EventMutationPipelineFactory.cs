using System.Collections.Generic;

namespace Journalist.EventStore.Events.Mutation
{
    public class EventMutationPipelineFactory : IEventMutationPipelineFactory
    {
        private readonly IReadOnlyCollection<IEventMutator> m_incomingMessageMutators;
        private readonly IReadOnlyCollection<IEventMutator> m_outgoingMessageMutators;

        public EventMutationPipelineFactory(
            IReadOnlyCollection<IEventMutator> incomingMessageMutators,
            IReadOnlyCollection<IEventMutator> outgoingMessageMutators)
        {
            Require.NotNull(incomingMessageMutators, "incomingMessageMutators");
            Require.NotNull(outgoingMessageMutators, "outgoingMessageMutators");

            m_incomingMessageMutators = incomingMessageMutators;
            m_outgoingMessageMutators = outgoingMessageMutators;
        }

        public IEventMutationPipeline CreateIncomingPipeline()
        {
            return new EventStreamMutationPipeline(m_incomingMessageMutators);
        }

        public IEventMutationPipeline CreateOutgoingPipeline()
        {
            return new EventStreamMutationPipeline(m_outgoingMessageMutators);
        }
    }
}