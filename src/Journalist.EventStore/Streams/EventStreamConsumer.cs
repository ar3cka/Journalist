using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumer : IEventStreamConsumer
    {
        public Task<bool> ReceiveEventsAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task RememberConsumedStreamVersionAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<JournaledEvent> Events
        {
            get;
            private set;
        }
    }
}
