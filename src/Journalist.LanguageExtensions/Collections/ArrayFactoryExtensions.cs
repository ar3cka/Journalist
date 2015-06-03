using System;

namespace Journalist.Collections
{
	public static class ArrayFactoryExtensions
	{
		public static T[] YieldArray<T>(this T item)
		{
			return new[] { item };
		}

		public static TTo[] YieldArray<TFrom, TTo>(this TFrom item)
			where TFrom : TTo
		{
			return new[] { (TTo)item };
		}

		public static TTo[] YieldArray<TFrom, TTo>(this TFrom item, Func<TFrom, TTo> select)
		{
			Require.NotNull(select, "select");

			return new[] { select(item) };
		}
	}
}