using System;
using System.IO;
using Newtonsoft.Json;

namespace Journalist.EventStore.Streams.Serializers.Json
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializer m_serializer;

        public JsonEventSerializer(JsonSerializer serializer)
        {
            Require.NotNull(serializer, "serializer");

            m_serializer = serializer;
        }

        public object Deserialize(StreamReader reader, Type eventType)
        {
            Require.NotNull(reader, "reader");
            Require.NotNull(eventType, "eventType");

            try
            {
                return m_serializer.Deserialize(reader, eventType);
            }
            catch (JsonSerializationException exception)
            {
                throw new EventSerializationException(
                    "Json event deserialization failed.",
                    exception);
            }
        }

        public void Serialize(object eventObject, Type eventType, StreamWriter writer)
        {
            Require.NotNull(eventObject, "eventObject");
            Require.NotNull(eventType, "eventType");
            Require.NotNull(writer, "writer");

            try
            {
                m_serializer.Serialize(writer, eventObject, eventType);
            }
            catch (JsonSerializationException exception)
            {
                throw new EventSerializationException(
                    "Json event serialization failed.",
                    exception);
            }
        }
    }
}
