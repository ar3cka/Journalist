using Journalist.Collections;
using Journalist.Extensions;
using Xunit;

namespace Journalist.LanguageExtensions.UnitTests.Collections
{
    public class CollectionExtensionsTests
    {
        [Fact]
        public void IsEmpty_WhenCollectionIsEmpty_ReturnsTrue()
        {
            Assert.True(EmptyArray.Get<int>().IsEmpty());
        }

        [Fact]
        public void IsEmpty_WhenCollectionIsNotEmpty_ReturnsFalse()
        {
            Assert.False(new[] { 1, 2, 3 }.IsEmpty());
        }

        [Fact]
        public void IsEmptyWithPredicate_WhenCollectionDoesNotContainElementsWithTruePredicate_ReturnsTrue()
        {
            Assert.True(new[] { 1, 2, 3 }.IsEmpty(value => value == 0));
        }

        [Fact]
        public void IsEmptyWithPredicate_WhenCollectionContainsElementsWithTruePredicate_ReturnsTrue()
        {
            Assert.False(new[] { 0, 1, 2 }.IsEmpty(value => value == 0));
        }
    }
}
