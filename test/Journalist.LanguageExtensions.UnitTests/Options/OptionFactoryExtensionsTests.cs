using Journalist.Options;
using Xunit;

namespace Journalist.LanguageExtensions.UnitTests.Options
{
    public class OptionFactoryExtensionsTests
    {
        [Fact]
        public void Maybe_OnNullableEnumWithNullValue_IsNone()
        {
            // arrange
            TestEnum? value = null;

            // act
            var option = value.MayBe();

            // assert
            Assert.True(option.IsNone);
        }

        [Fact]
        public void Maybe_OnNullableEnumWithNotNullValue_IsSome()
        {
            // arrange
            TestEnum? value = TestEnum.EnumValue1;

            // act
            var option = value.MayBe();

            // assert
            Assert.True(option.IsSome);
        }

        [Fact]
        public void Maybe_OnEnumValue_IsSome()
        {
            // arrange
            var value = TestEnum.EnumValue1;

            // act
            var option = value.MayBe();

            // assert
            Assert.True(option.IsSome);
        }

        private enum TestEnum
        {
            EnumValue1
        }
    }
}
