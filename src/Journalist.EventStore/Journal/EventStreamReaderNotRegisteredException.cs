using System;
using System.Runtime.Serialization;
using Journalist.Extensions;

namespace Journalist.EventStore.Journal
{
    [Serializable]
    public class EventStreamReaderNotRegisteredException : Exception
    {
        public EventStreamReaderNotRegisteredException(string streamName, EventStreamReaderId readerId)
            : this("Stream \"{0}\" reader \"{0}\" has not been registered.".FormatString(streamName, readerId))
        {
        }

        public EventStreamReaderNotRegisteredException()
        {
        }

        public EventStreamReaderNotRegisteredException(string message) : base(message)
        {
        }

        public EventStreamReaderNotRegisteredException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EventStreamReaderNotRegisteredException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
