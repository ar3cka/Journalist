using System;
using System.IO;

namespace Journalist.EventStore.Streams
{
    public interface IEventSerializer
    {
        object Deserialize(StreamReader reader, Type eventType);
    }
}
