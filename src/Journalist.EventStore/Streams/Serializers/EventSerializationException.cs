using System;
using System.Runtime.Serialization;

namespace Journalist.EventStore.Streams.Serializers
{
    [Serializable]
    public sealed class EventSerializationException : Exception
    {
        public EventSerializationException(string message) : base(message)
        {
        }

        public EventSerializationException(string message, Exception inner) : base(message, inner)
        {
        }

        private EventSerializationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
