using System;
using Xunit;

namespace Journalist.LanguageExtensions.UnitTests
{
    public class EnsureTests
    {
        [Fact]
        public void True_WhenConditionFalse_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => Ensure.True(false, "Error message."));
        }

        [Fact]
        public void GenericTrue_WhenConditionFalse_ThrowsSpecifiedExceptionType()
        {
            Assert.Throws<ArithmeticException>(
                () => Ensure.True<ArithmeticException>(false, "Error message."));
        }

        [Fact]
        public void False_WhenConditionTrue_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => Ensure.False(true, "Error message."));
        }

        [Fact]
        public void GenericFalse_WhenConditionTrue_ThrowsSpecifiedExceptionType()
        {
            Assert.Throws<ArithmeticException>(
                () => Ensure.False<ArithmeticException>(true, "Error message."));
        }
    }
}
