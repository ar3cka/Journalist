using System;

namespace Journalist.Options
{
    public static class OptionFactoryExtensions
    {
        public static Option<T> ToOption<T>(this T? value) where T : struct
        {
            return value.HasValue ? Option.Some(value.Value) : Option.None();
        }

        public static Option<TOut> ToOption<TIn, TOut>(this TIn? value, Func<TIn, TOut> map) where TIn : struct
        {
            return value.HasValue
                ? Option.Some(map(value.Value))
                : Option.None();
        }

        public static Option<T> MayBe<T>(this T value)
        {
            if (typeof (T).IsClass || typeof (T).IsInterface || typeof (T).IsValueType)
            {
                return value == null ? Option.None() : Option.Some(value);
            }

            return Option.Some(value);
        }

        public static Option<T> MayBe<T>(this T value, Predicate<T> isNone)
        {
            return isNone(value) ? Option.None() : Option.Some(value);
        }
    }
}
