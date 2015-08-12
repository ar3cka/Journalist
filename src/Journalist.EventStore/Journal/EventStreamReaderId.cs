using System;

namespace Journalist.EventStore.Journal
{
    public sealed class EventStreamReaderId : IEquatable<EventStreamReaderId>
    {
        private readonly Guid m_value;

        public EventStreamReaderId(Guid value)
        {
            Require.NotEmpty(value, "value");

            m_value = value;
        }

        public static EventStreamReaderId Create()
        {
            return new EventStreamReaderId(Guid.NewGuid());
        }

        public static EventStreamReaderId Parse(string consumerId)
        {
            return new EventStreamReaderId(Guid.Parse(consumerId));
        }

        public override string ToString()
        {
            return m_value.ToString("N").ToUpperInvariant();
        }

        public bool Equals(EventStreamReaderId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return m_value.Equals(other.m_value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is EventStreamReaderId && Equals((EventStreamReaderId)obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public static bool operator ==(EventStreamReaderId left, EventStreamReaderId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventStreamReaderId left, EventStreamReaderId right)
        {
            return !Equals(left, right);
        }
    }
}
