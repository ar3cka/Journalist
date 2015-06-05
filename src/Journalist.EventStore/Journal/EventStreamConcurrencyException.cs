using System;
using System.Runtime.Serialization;

namespace Journalist.EventStore.Journal
{
    [Serializable]
    public class EventStreamConcurrencyException : Exception
    {
        public EventStreamConcurrencyException()
        {
        }

        public EventStreamConcurrencyException(string message) : base(message)
        {
        }

        public EventStreamConcurrencyException(string message, Exception inner) : base(message, inner)
        {
        }

        private EventStreamConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}