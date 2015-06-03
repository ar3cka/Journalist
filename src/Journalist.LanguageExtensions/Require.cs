using System;
using System.Diagnostics;

namespace Journalist
{
    public static class Require
    {
        [DebuggerNonUserCode]
        public static void True(bool value, string param, string message)
        {
            if (value)
            {
                throw new ArgumentException(message, param);
            }
        }

        [DebuggerNonUserCode]
        public static void False(bool value, string param, string message)
        {
            if (value == false)
            {
                throw new ArgumentException(message, param);
            }
        }

        [DebuggerNonUserCode]
        public static void ZeroOrGreater(long value, string param)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(param, value, "Value must be zero or greater.");
            }
        }

        [DebuggerNonUserCode]
        public static void Positive(long value, string param)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(param, value, "Value must be greater than zero.");
            }
        }

        [DebuggerNonUserCode]
        public static void NotNull(object value, string param)
        {
            if (value == null)
            {
                throw new ArgumentNullException(param);
            }
        }

        [DebuggerNonUserCode]
        public static void NotEmpty(string value, string param)
        {
            True(string.IsNullOrEmpty(value) == false, param, "Value must be not empty.");
        }
    }
}
