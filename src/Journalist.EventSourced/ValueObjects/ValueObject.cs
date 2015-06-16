using System;
using System.Collections.Generic;

namespace Journalist.EventSourced.ValueObjects
{
    public abstract class ValueObject<T> : IEquatable<ValueObject<T>>
    {
        private readonly T m_value;

        protected ValueObject(T value, Func<T, string> errorMessage, Predicate<T> validate)
        {
            Require.NotNull(errorMessage, "errorMessage");
            Require.NotNull(validate, "validate");
            Require.True(validate(value), "value", errorMessage(value));

            m_value = value;
        }

        public bool Equals(ValueObject<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(Value, other.Value);
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ValueObject<T>)obj);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !Equals(left, right);
        }

        public T Value
        {
            get { return m_value; }
        }
    }
}
