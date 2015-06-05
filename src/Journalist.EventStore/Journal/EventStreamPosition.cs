using System;

namespace Journalist.EventStore.Journal
{
    public struct EventStreamPosition : IEquatable<EventStreamPosition>
    {
        public static readonly EventStreamPosition Start = new EventStreamPosition(string.Empty, StreamVersion.Zero);

        private readonly string m_etag;
        private readonly StreamVersion m_version;

        public EventStreamPosition(string etag, StreamVersion version)
        {
            Require.NotNull(etag, "etag");

            m_etag = etag;
            m_version = version;
        }

        public static bool IsAtStart(EventStreamPosition position)
        {
            return StreamVersion.IsZero(position.Version);
        }

        public bool Equals(EventStreamPosition other)
        {
            return string.Equals(m_etag, other.m_etag) && m_version.Equals(other.m_version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is EventStreamPosition && Equals((EventStreamPosition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return m_etag.GetHashCode() ^ m_version.GetHashCode();
            }
        }

        public static bool operator ==(EventStreamPosition left, EventStreamPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EventStreamPosition left, EventStreamPosition right)
        {
            return !left.Equals(right);
        }

        public StreamVersion Version
        {
            get { return m_version; }
        }

        public string ETag
        {
            get { return m_etag; }
        }
    }
}