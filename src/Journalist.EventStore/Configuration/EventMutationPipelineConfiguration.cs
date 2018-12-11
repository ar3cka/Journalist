using System.Collections.Generic;
using Journalist.EventStore.Events.Mutation;

namespace Journalist.EventStore.Configuration
{
    public class EventMutationPipelineConfiguration : IEventMutationPipelineConfiguration
    {
        private readonly IEventStoreConnectionConfiguration m_eventStoreConnectionConfiguration;
        private readonly List<IEventMutator> m_incomingMessageMutators;
        private readonly List<IEventMutator> m_outgoingMessageMutators;

        public EventMutationPipelineConfiguration(
            IEventStoreConnectionConfiguration eventStoreConnectionConfiguration,
            List<IEventMutator> incomingMessageMutators,
            List<IEventMutator> outgoingMessageMutators)
        {
            Require.NotNull(eventStoreConnectionConfiguration, "eventStoreConnectionConfiguration");
            Require.NotNull(incomingMessageMutators, "incomingMessageMutators");
            Require.NotNull(outgoingMessageMutators, "outgoingMessageMutators");

            m_eventStoreConnectionConfiguration = eventStoreConnectionConfiguration;
            m_incomingMessageMutators = incomingMessageMutators;
            m_outgoingMessageMutators = outgoingMessageMutators;
        }

        public IEventStoreConnectionConfiguration IncomingEventsWith<TEventMutator>(TEventMutator mutator)
            where TEventMutator : IEventMutator
        {
            m_incomingMessageMutators.Add(mutator);

            return m_eventStoreConnectionConfiguration;
        }

        public IEventStoreConnectionConfiguration OutgoingEventsWith<TEventMutator>(TEventMutator mutator)
            where TEventMutator : IEventMutator
        {
            m_outgoingMessageMutators.Add(mutator);

            return m_eventStoreConnectionConfiguration;
        }
    }
}
