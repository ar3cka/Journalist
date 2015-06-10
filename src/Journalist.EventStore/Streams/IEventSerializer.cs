using System;
using System.IO;

namespace Journalist.EventStore.Streams
{
    public interface IEventSerializer
    {
        object Deserialize(StreamReader reader, Type eventType);

        void Serialize(object eventObject, Type eventType, StreamWriter writer);
    }
}
