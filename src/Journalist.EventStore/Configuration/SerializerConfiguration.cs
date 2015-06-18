using Journalist.EventStore.Streams.Serializers.Json;
using Newtonsoft.Json;

namespace Journalist.EventStore.Configuration
{
    public static class SerializerConfiguration
    {
        public static IEventStoreConnectionConfiguration UseJsonSerializer(this IEventStoreConnectionConfiguration config)
        {
            Require.NotNull(config, "config");

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new OptionConverter());

            return config.UseSerializer(new JsonEventSerializer(serializer));
        }
    }
}
