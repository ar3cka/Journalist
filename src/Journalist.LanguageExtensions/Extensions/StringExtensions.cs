using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Journalist.Extensions
{
    public static class StringExtensions
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

        public static string JoinStringsWithCommas<T>(this IEnumerable<T> source, Func<T, string> toString)
        {
            Require.NotNull(source, "source");
            Require.NotNull(toString, "toString");

            return source.JoinStringsWith(", ", toString);
        }

        public static string JoinStringsWithCommas<T>(this IEnumerable<T> source)
        {
            Require.NotNull(source, "source");

            return source.JoinStringsWith(", ");
        }

        public static bool IsNotNullOrEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }
    }
}
