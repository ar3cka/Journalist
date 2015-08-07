using System;

namespace Journalist.EventStore.Notifications
{
    public sealed class NotificationId : IEquatable<NotificationId>
    {
        private readonly Guid m_value;

        public NotificationId(Guid value)
        {
            Require.NotEmpty(value, "value");

            m_value = value;
        }

        public static NotificationId Create()
        {
            return new NotificationId(Guid.NewGuid());
        }

        public static NotificationId Parse(string notificationId)
        {
            return new NotificationId(Guid.Parse(notificationId));
        }

        public override string ToString()
        {
            return m_value.ToString("N").ToUpperInvariant();
        }

        public bool Equals(NotificationId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return m_value.Equals(other.m_value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is NotificationId && Equals((NotificationId)obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public static bool operator ==(NotificationId left, NotificationId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NotificationId left, NotificationId right)
        {
            return !Equals(left, right);
        }
    }
}
