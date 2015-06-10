using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Journalist
{
    public static class Require
    {
        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void True(bool value, string param, string message)
        {
            if (value == false)
            {
                throw new ArgumentException(message, param);
            }
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void False(bool value, string param, string message)
        {
            if (value)
            {
                throw new ArgumentException(message, param);
            }
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroOrGreater(long value, string param)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(param, value, "Value must be zero or greater.");
            }
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Positive(long value, string param)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(param, value, "Value must be greater than zero.");
            }
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object value, string param)
        {
            if (value == null)
            {
                throw new ArgumentNullException(param);
            }
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(string value, string param)
        {
            False(string.IsNullOrEmpty(value), param, "Value must be not empty.");
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(Guid value, string param)
        {
            False(value == Guid.Empty, param, "Value must be not empty.");
        }

        [DebuggerNonUserCode]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty<T>(IEnumerable<T> value, string param)
        {
            True(value.Any(), param, "Collection must be not empty.");
        }
    }
}
