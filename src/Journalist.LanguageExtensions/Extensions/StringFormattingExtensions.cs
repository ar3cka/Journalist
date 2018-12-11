using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Journalist.Extensions
{
    public static class StringFormattingExtensions
    {
        public static string FormatString(this string format, params object[] values)
        {
            Require.NotEmpty(format, "format");
            Require.NotNull(values, "values");

            return string.Format(CultureInfo.InvariantCulture, format, values);
        }

        public static string JoinStringsWith<T>(this IEnumerable<T> source, string separator)
        {
            Require.NotNull(source, "source");
            Require.NotEmpty(separator, "separator");

            return string.Join(separator, source);
        }

        public static string JoinStringsWith<T>(this IEnumerable<T> source, string separator, Func<T, string> toString)
        {
            Require.NotNull(source, "source");
            Require.NotEmpty(separator, "separator");
            Require.NotNull(toString, "toString");

            return string.Join(separator, source.Select(toString));
        }

        public static string ToInvariantString(this IFormattable value)
        {
            Require.NotNull(value, "value");

            return value.ToString(null, CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this TimeSpan value)
        {
            return value.ToString("c");
        }

        public static string ToInvariantString(this DateTime value)
        {
            return value.ToString("O");
        }

        public static string ToInvariantString(this DateTimeOffset value)
        {
            return value.ToString("O");
        }

        public static string ToCsvString<T>(this IEnumerable<T> source, Func<T, string> toString)
        {
            Require.NotNull(source, "source");
            Require.NotNull(toString, "toString");

            return source.JoinStringsWith(", ", toString);
        }

        public static string ToCsvString<T>(this IEnumerable<T> source)
        {
            Require.NotNull(source, "source");

            return source.JoinStringsWith(", ");
        }
    }
}
