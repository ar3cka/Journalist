using System;
using System.Collections.Generic;

namespace Journalist.Collections
{
    public static class ListFactoryExtensions
    {
        public static List<T> YieldList<T>(this T item)
        {
            return new List<T> { item };
        }

        public static List<TTo> YieldList<TFrom, TTo>(this TFrom item)
            where TFrom : TTo
        {
            return new List<TTo> { item };
        }

        public static List<TTo> YieldList<TFrom, TTo>(this TFrom item, Func<TFrom, TTo> select)
        {
            Require.NotNull(select, "select");

            return new List<TTo> { select(item) };
        }
    }
}