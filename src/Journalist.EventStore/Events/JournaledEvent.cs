using System;
using System.Collections.Generic;
using System.IO;

namespace Journalist.EventStore.Events
{
    public sealed class JournaledEvent : IEquatable<JournaledEvent>
    {
        private JournaledEvent()
        {
        }

        public static JournaledEvent Create(
            Guid eventId,
            object eventObject,
            Action<object, Type, StreamWriter> serialize)
        {
            Require.NotEmpty(eventId, "eventObject");
            Require.NotNull(eventObject, "eventObject");
            Require.NotNull(serialize, "serialize");

            var eventType = eventObject.GetType();
            MemoryStream payload;
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                serialize(eventObject, eventType, writer);
                writer.Flush();

                payload = new MemoryStream(stream.GetBuffer(), 0, (int) stream.Length, false);
            }

            return new JournaledEvent
            {
                EventId = eventId,
                EventTypeName = eventType.AssemblyQualifiedName,
                EventPayload = payload,
            };
        }

        public static JournaledEvent Create(object eventObject, Action<object, Type, StreamWriter> serialize)
        {
            return Create(Guid.NewGuid(), eventObject, serialize);
        }

        public static JournaledEvent Create(IDictionary<string, object> properties)
        {
            Require.NotNull(properties, "properties");

            var result = new JournaledEvent
            {
                EventId = (Guid)properties[JournaledEventPropertyNames.EventId],
                EventTypeName = (string)properties[JournaledEventPropertyNames.EventType],
                EventPayload = (Stream)properties[JournaledEventPropertyNames.EventPayload]
            };

            return result;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();
            result[JournaledEventPropertyNames.EventId] = EventId;
            result[JournaledEventPropertyNames.EventType] = EventTypeName;
            result[JournaledEventPropertyNames.EventPayload] = EventPayload;

            return result;
        }

        public bool Equals(JournaledEvent other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EventId.Equals(other.EventId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is JournaledEvent && Equals((JournaledEvent) obj);
        }

        public override int GetHashCode()
        {
            return EventId.GetHashCode();
        }

        public Type EventType
        {
            get { return Type.GetType(EventTypeName, true); }
        }

        public Guid EventId { get; private set; }

        public string EventTypeName { get; private set; }

        public Stream EventPayload { get; private set; }
    }
}
