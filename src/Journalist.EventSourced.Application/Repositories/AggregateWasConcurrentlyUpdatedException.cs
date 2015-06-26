using System;
using System.Runtime.Serialization;

namespace Journalist.EventSourced.Application.Repositories
{
    [Serializable]
    public sealed class AggregateWasConcurrentlyUpdatedException : Exception
    {
        public AggregateWasConcurrentlyUpdatedException()
        {
        }

        public AggregateWasConcurrentlyUpdatedException(string message) : base(message)
        {
        }

        public AggregateWasConcurrentlyUpdatedException(string message, Exception inner) : base(message, inner)
        {
        }

        private AggregateWasConcurrentlyUpdatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
