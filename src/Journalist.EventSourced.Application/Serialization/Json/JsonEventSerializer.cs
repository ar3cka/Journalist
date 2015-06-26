using System.IO;
using Journalist.EventStore.Events;
using Newtonsoft.Json;

namespace Journalist.EventSourced.Application.Serialization.Json
{
    public class JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializer m_serializer;

        public JsonEventSerializer()
        {
            m_serializer = new JsonSerializer();
            m_serializer.Converters.Add(new OptionConverter());
        }

        public object Deserialize(JournaledEvent journaledEvent)
        {
            Require.NotNull(journaledEvent, "journaledEvent");

            using (var streamReader = new StreamReader(journaledEvent.EventPayload))
            using (var jsonreader = new JsonTextReader(streamReader))
            {
                return m_serializer.Deserialize(jsonreader, journaledEvent.EventType);
            }
        }

        public JournaledEvent Serialize(object change)
        {
            Require.NotNull(change, "change");

            return JournaledEvent.Create(change, (eventObj, type, writer) => m_serializer.Serialize(writer, eventObj));
        }
    }
}
