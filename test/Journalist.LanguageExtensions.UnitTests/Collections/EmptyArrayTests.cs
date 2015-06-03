using Journalist.Collections;
using Xunit;

namespace Journalist.LanguageExtensions.UnitTests.Collections
{
    public class EmptyArrayTests
    {
        [Fact]
        public void Get_ReturnsSameInstance()
        {
            Assert.Same(EmptyArray.Get<int>(), EmptyArray.Get<int>());
        }
    }
}