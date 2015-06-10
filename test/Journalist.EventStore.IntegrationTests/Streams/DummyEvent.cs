using System;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class DummyEvent : IEquatable<DummyEvent>
    {
        public Guid EventContent { get; set; }

        public bool Equals(DummyEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EventContent.Equals(other.EventContent);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DummyEvent) obj);
        }

        public override int GetHashCode()
        {
            return EventContent.GetHashCode();
        }
    }
}
