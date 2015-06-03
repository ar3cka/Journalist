using System;

namespace Journalist.Options
{
	public static class OptionExtensions
	{
		private static readonly object VoidValue = new object();

		public static TResult Match<T, TResult>(this Option<T> source, Func<T, TResult> map, TResult missingValue)
		{
			Require.NotNull(map, "map");

			return source.Match(map, () => missingValue);
		}

		public static Option<TOut> Bind<TIn, TOut>(this Option<TIn> source, Func<TIn, Option<TOut>> function)
		{
			Require.NotNull(function, "function");

			return source.Match(function, () => Option.None());
		}

		public static Option<TOut> Select<TIn, TOut>(this Option<TIn> source, Func<TIn, TOut> function)
		{
			Require.NotNull(function, "function");

			return source.Bind(value => function(value).MayBe());
		}

		public static Option<TOut> SelectMany<TIn, TIntermediate, TOut>(
			this Option<TIn> source,
			Func<TIn, Option<TIntermediate>> select,
			Func<TIn, TIntermediate, TOut> selectOut)
		{
			Require.NotNull(select, "select");
			Require.NotNull(selectOut, "selectOut");

			return source.Bind(s => select(s).Select(m => selectOut(s, m)));
		}

		public static Option<T> Where<T>(this Option<T> value, Func<T, bool> filter)
		{
			Require.NotNull(filter, "filter");

			return value.Bind(e => filter(e) ? e.MayBe() : Option.None());
		}

		public static Option<TTo> Cast<TFrom, TTo>(this Option<TFrom> value)
		{
			return value.Bind(e => ((TTo)Convert.ChangeType(e, typeof(TTo))).MayBe());
		}

		public static Option<T> Unwrap<T>(this Option<Option<T>> value)
		{
			return value.Bind(e => e);
		}

		public static T GetOrDefault<T>(this Option<T> source, Func<T> defaultValue)
		{
			Require.NotNull(defaultValue, "defaultValue");

			return source.Match(value => value, defaultValue);
		}

		public static T GetOrDefault<T>(this Option<T> source, T defaultValue)
		{
			return source.GetOrDefault(() => defaultValue);
		}

		public static T GetOrThrow<T>(this Option<T> source, Func<Exception> exception)
		{
			Require.NotNull(exception, "exception");

			return source.Match(
				value => value,
				() => { throw exception(); });
		}

		public static T GetOrThrow<T>(this Option<T> source, Exception exception)
		{
			Require.NotNull(exception, "exception");

			return source.GetOrThrow(() => exception);
		}

		public static void Do<T>(this Option<T> source, Action<T> action)
		{
			Require.NotNull(source, "source");

			source.Match(
				value => { action(value); return VoidValue; },
				VoidValue);
		}
	}
}