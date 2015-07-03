using System.IO;
using Journalist.Collections;

namespace Journalist.IO
{
    public static class EmptyMemoryStream
    {
        private static readonly MemoryStream s_instance = new MemoryStream(
            EmptyArray.Get<byte>(),
            false);

        public static MemoryStream Get()
        {
            return s_instance;
        }
    }
}
