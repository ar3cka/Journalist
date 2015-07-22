using System;

namespace Journalist.EventStore.Streams
{
    public sealed class EventStreamConsumerId : IEquatable<EventStreamConsumerId>
    {
        private readonly Guid m_value;

        public EventStreamConsumerId(Guid value)
        {
            Require.NotEmpty(value, "value");

            m_value = value;
        }

        public static EventStreamConsumerId Create()
        {
            return new EventStreamConsumerId(Guid.NewGuid());
        }

        public static EventStreamConsumerId Parse(string consumerId)
        {
            return new EventStreamConsumerId(Guid.Parse(consumerId));
        }

        public override string ToString()
        {
            return m_value.ToString("N").ToUpperInvariant();
        }

        public bool Equals(EventStreamConsumerId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return m_value.Equals(other.m_value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is EventStreamConsumerId && Equals((EventStreamConsumerId)obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public static bool operator ==(EventStreamConsumerId left, EventStreamConsumerId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventStreamConsumerId left, EventStreamConsumerId right)
        {
            return !Equals(left, right);
        }
    }
}
