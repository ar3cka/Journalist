using Journalist.Options;
using Xunit;

namespace Journalist.LanguageExtensions.UnitTests.Options
{
	public class OptionTests
	{
		[Fact]
		public void Equals_EqualsValues_ReturnsTrue()
		{
			var a = Option.Some(1);
			var b = Option.Some(1);

			Assert.True(a.Equals(b));
		}

		[Fact]
		public void Equals_NotEqualsValues_ReturnsFalse()
		{
			var a = Option.Some(1);
			var b = Option.Some(2);

			Assert.False(a.Equals(b));
		}

		[Fact]
		public void Equals_ForEqualsBoxedValues_ReturnsTrue()
		{
			object a = Option.Some(1);
			object b = Option.Some(1);

			Assert.True(a.Equals(b));
		}

		[Fact]
		public void Equals_ForNotEqualsBoxedValues_ReturnsFalse()
		{
			object a = Option.Some(1);
			object b = Option.Some(2);

			Assert.False(a.Equals(b));
		}

		[Fact]
		public void Equals_ForNoneAndSomeValues_ReturnsFalse()
		{
			var a = Option.Some(1);
			var b = Option.None();
			Assert.False(a.Equals(b));
		}

		[Fact]
		public void Equals_ForNoneValues_ReturnsTrue()
		{
			var a = Option.None();
			var b = Option.None();

			Assert.True(a.Equals(b));
		}

		[Fact]
		public void MayBe_ForNull_ReturnsNoneOption()
		{
			object a = null;
			Assert.Equal(Option.None(), a.MayBe());
		}

		[Fact]
		public void MayBe_ForStruct_ReturnsSomeOption()
		{
			int? a = 10;
			Assert.NotEqual(Option.None(), a.MayBe());
		}
	}
}
