using System;
using System.Runtime.Serialization;

namespace Journalist.EventStore.Streams
{
    [Serializable]
    public sealed class EventStreamConnectionWasClosedException : Exception
    {
        public EventStreamConnectionWasClosedException()
            : this("Underlying event stream connection was closed.")
        {
        }

        public EventStreamConnectionWasClosedException(string message) : base(message)
        {
        }

        public EventStreamConnectionWasClosedException(string message, Exception inner) : base(message, inner)
        {
        }

        private EventStreamConnectionWasClosedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
