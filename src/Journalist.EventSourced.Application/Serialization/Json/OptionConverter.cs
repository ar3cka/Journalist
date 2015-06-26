using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Journalist.Collections;
using Journalist.Options;
using Newtonsoft.Json;

namespace Journalist.EventSourced.Application.Serialization.Json
{
    public class OptionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var optionType = value.GetType();

            var optionWriter = Writers.GetOrAdd(optionType, _ => new OptionWriter(optionType));
            optionWriter.Serialize(value, writer, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var optionReader = Readers.GetOrAdd(objectType, _ => new OptionReader(objectType));

            var rawValue = optionReader.Deserialize(reader, serializer);
            var result = optionReader.SomeValue(rawValue);

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>);
        }

        private static readonly ConcurrentDictionary<Type, OptionReader> Readers = new ConcurrentDictionary<Type, OptionReader>();
        private static readonly ConcurrentDictionary<Type, OptionWriter> Writers = new ConcurrentDictionary<Type, OptionWriter>();

        private class OptionReader
        {
            public readonly Func<JsonReader, JsonSerializer, object> Deserialize;
            public readonly Func<object, object> SomeValue;

            public OptionReader(Type optionType)
            {
                var rawType = optionType.GetGenericArguments()[0];
                if (rawType.IsClass)
                {
                    Deserialize = (reader, serializer) => serializer.Deserialize(reader, rawType);
                }
                else
                {
                    Deserialize = (reader, serializer) => serializer.Deserialize(reader, typeof(Nullable<>).MakeGenericType(rawType));
                }

                var noneField = optionType.GetField("None", BindingFlags.Static | BindingFlags.NonPublic);
                Debug.Assert(noneField != null);
                var noneResult = noneField.GetValue(null);

                var ctor = optionType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    rawType.YieldArray(),
                    null);

                SomeValue = rawValue => rawValue == null ? noneResult : ctor.Invoke(rawValue.YieldArray());
            }
        }

        private class OptionWriter
        {
            public readonly Action<object, JsonWriter, JsonSerializer> Serialize;

            public OptionWriter(Type optionType)
            {
                var hasValueField = optionType.GetField("m_hasValue", BindingFlags.Instance | BindingFlags.NonPublic);
                var valueField = optionType.GetField("m_value", BindingFlags.Instance | BindingFlags.NonPublic);

                Debug.Assert(hasValueField != null);
                Debug.Assert(valueField != null);

                Serialize = (value, writer, serializer) =>
                {
                    if ((bool)hasValueField.GetValue(value))
                    {
                        var rawValue = valueField.GetValue(value);
                        serializer.Serialize(writer, rawValue);
                    }
                    else
                    {
                        writer.WriteNull();
                    }
                };
            }
        }
    }
}
