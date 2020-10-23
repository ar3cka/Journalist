using System;
using System.Globalization;

namespace Journalist.Options
{
    [Serializable]
    public struct Option<T> : IEquatable<Option<T>>, IEquatable<Option.NoneOption>
    {
        private static readonly Option<T> None = new Option<T>();

        private readonly bool m_hasValue;
        private readonly T m_value;

        internal Option(T value)
        {
            m_hasValue = true;
            m_value = value;
        }

        public TResult Match<TResult>(Func<T, TResult> map, Func<TResult> missingValue)
        {
            Require.NotNull(map, "map");
            Require.NotNull(missingValue, "missingValue");

            return m_hasValue ? map(m_value) : missingValue();
        }

        public bool IsNone
        {
            get { return !m_hasValue; }
        }

        public bool IsSome
        {
            get { return m_hasValue; }
        }

        public static implicit operator Option<T>(Option.NoneOption noValue)
        {
            return None;
        }

        public bool Equals(Option<T> other)
        {
            if (m_hasValue != other.m_hasValue)
            {
                return false;
            }

            return !m_hasValue || Equals(m_value, other.m_value);
        }

        public bool Equals(Option.NoneOption other)
        {
            return !m_hasValue;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (other is Option<T>)
            {
                return Equals((Option<T>) other);
            }

            if (other is Option.NoneOption)
            {
                return Equals((Option.NoneOption) other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_hasValue
                ? ReferenceEquals(m_value, null) ? -1 : m_value.GetHashCode()
                : 0;
        }

        public override string ToString()
        {
            return m_hasValue
                ? string.Format(CultureInfo.InvariantCulture, "Value: {0}", m_value)
                : "No Value";
        }

        public static bool operator ==(Option<T> left, Option<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Option<T> left, Option<T> right)
        {
            return !left.Equals(right);
        }
    }

    public static class Option
    {
        [Serializable]
        public struct NoneOption
        {
            public override bool Equals(object obj)
            {
                var type = obj.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Option<>))
                {
                    return obj.Equals(this);
                }

                if (obj is NoneOption)
                {
                    return true;
                }

                return base.Equals(obj);
            }

            public bool IsNone
            {
                get { return true; }
            }

            public bool IsSome
            {
                get { return false; }
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        public static Option<T> Some<T>(T value)
        {
            if (typeof(T).IsClass || typeof(T).IsInterface || typeof(T).IsValueType)
            {
                Require.True(value != null, "value", "Can not create Option.Some for 'null' value of referenced type.");
            }

            return new Option<T>(value);
        }

        public static NoneOption None()
        {
            return new NoneOption();
        }
    }
}