using System;
using System.Runtime.Serialization;

namespace Journalist.EventStore.Journal
{
    [Serializable]
    public class EventStreamAppendOperationFailedException : Exception
    {
        public EventStreamAppendOperationFailedException()
        {
        }

        public EventStreamAppendOperationFailedException(string message)
            : base(message)
        {
        }

        public EventStreamAppendOperationFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        private EventStreamAppendOperationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}