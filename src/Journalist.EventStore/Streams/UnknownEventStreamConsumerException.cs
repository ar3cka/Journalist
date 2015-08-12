using System;
using System.Runtime.Serialization;
using Journalist.EventStore.Journal;
using Journalist.Extensions;

namespace Journalist.EventStore.Streams
{
    [Serializable]
    public sealed class UnknownEventStreamConsumerException : Exception
    {
        public UnknownEventStreamConsumerException()
        {
        }

        public UnknownEventStreamConsumerException(EventStreamReaderId consumerId)
            : this("Event stream consumer with id \"{0}\" not registered.".FormatString(consumerId))
        {
        }

        public UnknownEventStreamConsumerException(string message) : base(message)
        {
        }

        public UnknownEventStreamConsumerException(string message, Exception inner) : base(message, inner)
        {
        }

        private UnknownEventStreamConsumerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
