using System.Collections.Generic;
using System.Linq;

namespace Journalist.EventStore.Events.Mutation
{
    public class EventStreamMutationPipeline : IEventMutationPipeline
    {
        private readonly IReadOnlyCollection<IEventMutator> m_mutators;

        public EventStreamMutationPipeline(IReadOnlyCollection<IEventMutator> mutators)
        {
            Require.NotNull(mutators, "mutators");

            m_mutators = mutators;
        }

        public JournaledEvent Mutate(JournaledEvent journaledEvent)
        {
            Require.NotNull(journaledEvent, "journaledEvent");

            return m_mutators.Aggregate(journaledEvent, (current, mutator) => mutator.Mutate(current));
        }
    }
}
