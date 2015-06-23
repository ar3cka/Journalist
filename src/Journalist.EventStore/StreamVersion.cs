using System;

namespace Journalist.EventStore
{
    public struct StreamVersion : IEquatable<StreamVersion>, IComparable<StreamVersion>
    {
        private readonly int m_value;

        public static readonly StreamVersion Unknown = new StreamVersion(0);
        public static readonly StreamVersion Start = new StreamVersion(1);

        private StreamVersion(int value)
        {
            m_value = value;
        }

        public static bool IsUnknown(StreamVersion version)
        {
            return version.m_value == 0;
        }

        public static StreamVersion Create(int version)
        {
            Require.Positive(version, "value");

            return new StreamVersion(version);
        }

        public static StreamVersion Parse(string version)
        {
            Require.NotEmpty(version, "version");

            return Create(int.Parse(version));
        }

        public StreamVersion Increment(int incrementValue)
        {
            return Create(m_value + incrementValue);
        }

        public StreamVersion Increment()
        {
            return Increment(1);
        }

        public StreamVersion Decrement(int decrementValue)
        {
            return Create(m_value - decrementValue);
        }

        public StreamVersion Decrement()
        {
            return Decrement(1);
        }

        public bool Equals(StreamVersion other)
        {
            return m_value == other.m_value;
        }

        public int CompareTo(StreamVersion other)
        {
            return m_value.CompareTo(other.m_value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is StreamVersion && Equals((StreamVersion) obj);
        }

        public override int GetHashCode()
        {
            return m_value;
        }

        public override string ToString()
        {
            return m_value.ToString("0000000000");
        }

        public static bool operator ==(StreamVersion left, StreamVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StreamVersion left, StreamVersion right)
        {
            return !left.Equals(right);
        }

        public static bool operator >=(StreamVersion left, StreamVersion right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(StreamVersion left, StreamVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(StreamVersion left, StreamVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(StreamVersion left, StreamVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        public static explicit operator int(StreamVersion version)
        {
            return version.m_value;
        }
    }
}
