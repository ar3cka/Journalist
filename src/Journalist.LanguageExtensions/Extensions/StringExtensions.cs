using System;

namespace Journalist.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsCs(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.Ordinal);
        }

        public static bool EqualsCi<T>(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
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
