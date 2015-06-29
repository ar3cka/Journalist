using System;
using System.Collections.Generic;
using System.IO;

namespace Journalist.EventStore.Events
{
    public sealed class JournaledEvent : IEquatable<JournaledEvent>
    {
        private readonly MemoryStream m_eventPayload;
        private readonly string m_eventTypeName;
        private readonly Guid m_eventId;

        private JournaledEvent(Guid eventId, string eventTypeName, MemoryStream eventPayload)
        {
            m_eventId = eventId;
            m_eventTypeName = eventTypeName;
            m_eventPayload = eventPayload;
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

                payload = new MemoryStream(
                    buffer: stream.GetBuffer(),
                    index: 0,
                    count: (int)stream.Length,
                    writable: false,
                    publiclyVisible: true);
            }

            return new JournaledEvent(eventId, eventType.AssemblyQualifiedName, payload);
        }

        public static JournaledEvent Create(object eventObject, Action<object, Type, StreamWriter> serialize)
        {
            return Create(Guid.NewGuid(), eventObject, serialize);
        }

        public static JournaledEvent Create(IDictionary<string, object> properties)
        {
            Require.NotNull(properties, "properties");

            var payload  = new MemoryStream();
            ((MemoryStream)properties[JournaledEventPropertyNames.EventPayload]).CopyTo(payload);

            return new JournaledEvent(
                (Guid)properties[JournaledEventPropertyNames.EventId],
                (string)properties[JournaledEventPropertyNames.EventType],
                payload);
        }

        public MemoryStream GetEventPayload()
        {
            return new MemoryStream(
                buffer: m_eventPayload.GetBuffer(),
                index: 0,
                count: (int)m_eventPayload.Length,
                writable: false,
                publiclyVisible: false);
        }

        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();
            result[JournaledEventPropertyNames.EventId] = m_eventId;
            result[JournaledEventPropertyNames.EventType] = m_eventTypeName;
            result[JournaledEventPropertyNames.EventPayload] = GetEventPayload();

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

            return m_eventId.Equals(other.m_eventId);
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

            return obj is JournaledEvent && Equals((JournaledEvent)obj);
        }

        public override int GetHashCode()
        {
            return m_eventId.GetHashCode();
        }

        public Type EventType
        {
            get { return Type.GetType(EventTypeName, true); }
        }

        public Guid EventId
        {
            get { return m_eventId; }
        }

        public string EventTypeName
        {
            get { return m_eventTypeName; }
        }
    }
}
