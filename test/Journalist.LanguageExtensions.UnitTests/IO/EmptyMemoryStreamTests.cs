using System.IO;
using Journalist.IO;
using Xunit;

namespace Journalist.LanguageExtensions.UnitTests.IO
{
    public class EmptyMemoryStreamTests
    {
        [Fact]
        public void EmptyMemoryStream_CanBeReused()
        {
            using (var reader = new StreamReader(EmptyMemoryStream.Get()))
            {
                reader.ReadToEnd();
            }

            using (var reader = new StreamReader(EmptyMemoryStream.Get()))
            {
                reader.ReadToEnd();
            }
        }

        [Fact]
        public void EmptyMemoryStream_NoWritable()
        {
            Assert.False(EmptyMemoryStream.Get().CanWrite);
        }
    }
}
