using System;
using System.IO;
using Journalist.EventStore.Streams.Serializers.Json;
using Journalist.Options;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Xunit;

namespace Journalist.EventStore.UnitTests.Streams.Serializers
{
    public class OptionConverterTests
    {
        public OptionConverterTests()
        {
            Serializer = new JsonSerializer();
            Serializer.Converters.Add(new OptionConverter());

            Fixture = new Fixture();

            Fixture.Customize<Option<Guid>>(composer => composer
                .FromFactory((Guid value) => Option.Some(value)));
        }

        [Fact]
        public void SerializedOption_CanBeDeserialized()
        {
            var originalValue = Fixture.Create<MyEntity>();

            var bytes = Serialize(originalValue);
            var restoredValue = Deserialize<MyEntity>(bytes);

            Assert.Equal(originalValue.Value, restoredValue.Value);
        }

        private T Deserialize<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new StreamReader(stream))
            {
                return (T)Serializer.Deserialize(reader, typeof(T));
            }
        }

        private byte[] Serialize<T>(T value)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                Serializer.Serialize(writer, value);

                writer.Flush();

                return stream.ToArray();
            }
        }

        public JsonSerializer Serializer { get; set; }

        public IFixture Fixture { get; set; }

        public class MyEntity
        {
            public Option<Guid> Value { get; set; }
        }
    }
}
