using System;
using System.Collections.Generic;
using System.Linq;

namespace Journalist.Extensions
{
    public static class CollectionExtensions
    {
        public static TTo[] SelectToArray<TFrom, TTo>(this IEnumerable<TFrom> collection, Func<TFrom, TTo> map)
        {
            Require.NotNull(collection, "collection");
            Require.NotNull(map, "map");

            return collection.Select(map).ToArray();
        }

        public static TResult[] SelectToArray<TKey, TValue, TResult>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source,
            Func<TKey, TValue, TResult> select)
        {
            Require.NotNull(source, "source");
            Require.NotNull(select, "selectKey");

            return source
                .Select(pair => select(pair.Key, pair.Value))
                .ToArray();
        }

        public static List<TTo> SelectToList<TFrom, TTo>(this IEnumerable<TFrom> collection, Func<TFrom, TTo> map)
        {
            Require.NotNull(collection, "collection");
            Require.NotNull(map, "map");

            return collection.Select(map).ToList();
        }

        public static TResult[] SelectToList<TKey, TValue, TResult>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source,
            Func<TKey, TValue, TResult> select)
        {
            Require.NotNull(source, "source");
            Require.NotNull(select, "selectKey");

            return source
                .Select(pair => select(pair.Key, pair.Value))
                .ToArray();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            Require.NotNull(collection, "collection");

            return !collection.Any();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            Require.NotNull(collection, "collection");
            Require.NotNull(predicate, "predicate");

            return !collection.Any(predicate);
        }
    }
}
