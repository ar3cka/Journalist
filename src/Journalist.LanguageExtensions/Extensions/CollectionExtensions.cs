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

        public static List<TTo> SelectToList<TFrom, TTo>(this IEnumerable<TFrom> collection, Func<TFrom, TTo> map)
        {
            Require.NotNull(collection, "collection");
            Require.NotNull(map, "map");

            return collection.Select(map).ToList();
        }
    }
}
