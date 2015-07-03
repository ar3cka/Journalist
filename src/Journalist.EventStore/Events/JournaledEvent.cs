using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Journalist.Collections;
using Journalist.Extensions;

namespace Journalist.EventStore.Events
{
    public sealed class JournaledEvent : IEquatable<JournaledEvent>
    {
        private readonly MemoryStream m_eventPayload;
        private readonly Dictionary<string, string> m_eventHeaders;
        private readonly string m_eventTypeName;
        private readonly Guid m_eventId;

        private JournaledEvent(
            Guid eventId,
            string eventTypeName,
            Dictionary<string, string> eventHeaders,
            MemoryStream eventPayload)
        {
            m_eventId = eventId;
            m_eventTypeName = eventTypeName;
            m_eventPayload = eventPayload;
            m_eventHeaders = eventHeaders;
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

            MemoryStream payloadBytes;
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                serialize(eventObject, eventType, writer);
                writer.Flush();

                payloadBytes = new MemoryStream(
                    buffer: stream.GetBuffer(),
                    index: 0,
                    count: (int)stream.Length,
                    writable: false,
                    publiclyVisible: true);
            }

            return new JournaledEvent(
                eventId,
                eventType.AssemblyQualifiedName,
                new Dictionary<string, string>(),
                payloadBytes);
        }

        public static JournaledEvent Create(object eventObject, Action<object, Type, StreamWriter> serialize)
        {
            return Create(Guid.NewGuid(), eventObject, serialize);
        }

        public static JournaledEvent Create(IDictionary<string, object> properties)
        {
            Require.NotNull(properties, "properties");

            var payload  = new MemoryStream();
            ((Stream)properties[JournaledEventPropertyNames.EventPayload]).CopyTo(payload);

            return new JournaledEvent(
                (Guid)properties[JournaledEventPropertyNames.EventId],
                (string)properties[JournaledEventPropertyNames.EventType],
                ParseHeaders((string)properties[JournaledEventPropertyNames.EventHeaders]),
                payload);
        }

        public void SetHeader(string headerName, string headerValue)
        {
            Require.NotEmpty(headerName, "headerName");

            if (headerValue.IsNullOrEmpty() && m_eventHeaders.ContainsKey(headerName))
            {
                m_eventHeaders.Remove(headerName);
                return;
            }

            m_eventHeaders[headerName] = headerValue;
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
            result[JournaledEventPropertyNames.EventHeaders] = FormatHeaders();

            return result;
        }

        private string FormatHeaders()
        {
            if (m_eventHeaders.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var eventHeader in m_eventHeaders)
            {
                builder.AppendFormat("{0}: {1}\r\n", eventHeader.Key, eventHeader.Value);
            }

            return builder.ToString();
        }

        private static Dictionary<string, string> ParseHeaders(string formattedHeaders)
        {
            if (formattedHeaders.IsNullOrEmpty())
            {
                return new Dictionary<string, string>(0);
            }

            var result = new Dictionary<string, string>();
            var newline = "\r\n";
            var pairs = formattedHeaders.Split(newline.YieldArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split(": ".YieldArray(), StringSplitOptions.RemoveEmptyEntries);
                result.Add(keyValue[0], keyValue[1]);
            }

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

        public IReadOnlyDictionary<string, string> Headers
        {
            get { return m_eventHeaders; }
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
