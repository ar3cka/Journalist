namespace Journalist.Collections
{
	public static class EmptyArray
	{
		public static T[] Get<T>()
		{
			return EmptyArrayInstance<T>.Instance;
		}

		private static class EmptyArrayInstance<T>
		{
			internal static readonly T[] Instance = new T[0];
		}
	}
}
