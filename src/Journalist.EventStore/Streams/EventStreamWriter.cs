using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
    public class EventStreamWriter : IEventStreamWriter
    {
        public Task AppendEvents(IEnumerable<object> events)
        {
            throw new System.NotImplementedException();
        }

        public int StreamPosition
        {
            get; private set;
        }
    }
}
