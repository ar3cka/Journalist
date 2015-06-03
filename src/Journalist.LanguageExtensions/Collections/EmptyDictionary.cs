using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Journalist.Collections
{
	public static class EmptyDictionary
	{
		public static IReadOnlyDictionary<TKey, TValue> Get<TKey, TValue>()
		{
			return EmptyDictInstance<TKey, TValue>.Instance;
		}

		private static class EmptyDictInstance<TKey, TValue>
		{
			internal static readonly IReadOnlyDictionary<TKey, TValue> Instance =
				new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>(0));
		}
	}
}