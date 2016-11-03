using System;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal
{
    public struct EventStreamHeader : IEquatable<EventStreamHeader>
    {
        public static readonly EventStreamHeader Unknown = new EventStreamHeader(string.Empty, StreamVersion.Unknown);

        private readonly string m_etag;
        private readonly StreamVersion m_version;

        public EventStreamHeader(string etag, StreamVersion version)
        {
            Require.NotNull(etag, "etag");

            m_etag = etag;
            m_version = version;
        }

        public static bool IsNewStream(EventStreamHeader header)
        {
            return StreamVersion.IsUnknown(header.Version);
        }

        public bool Equals(EventStreamHeader other)
        {
            return string.Equals(m_etag, other.m_etag) && m_version.Equals(other.m_version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is EventStreamHeader && Equals((EventStreamHeader) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return m_etag.GetHashCode() ^ m_version.GetHashCode();
            }
        }

        public static bool operator ==(EventStreamHeader left, EventStreamHeader right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EventStreamHeader left, EventStreamHeader right)
        {
            return !left.Equals(right);
        }

        public StreamVersion Version => m_version;

        public string ETag => m_etag;
    }
}
