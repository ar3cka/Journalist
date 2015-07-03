using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class MessageMutator : IEventMutator
    {
        private readonly string m_headerName;

        public MessageMutator(string headerName)
        {
            m_headerName = headerName;
        }

        public JournaledEvent Mutate(JournaledEvent journaledEvent)
        {
            journaledEvent.SetHeader(m_headerName, "A");

            return journaledEvent;
        }
    }
}
