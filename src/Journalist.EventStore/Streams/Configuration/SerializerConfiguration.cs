using Journalist.EventStore.Streams.Serializers.Json;
using Newtonsoft.Json;

namespace Journalist.EventStore.Streams.Configuration
{
    public static class SerializerConfiguration
    {
        public static IEventStreamConfiguration UseJsonSerializer(
            this IEventStreamConfiguration config)
        {
            Require.NotNull(config, "config");

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new OptionConverter());

            return config.UseSerializer(new JsonEventSerializer(serializer));
        }
    }
}
