using System.IO;
using Journalist.Collections;

namespace Journalist.IO
{
    public static class EmptyMemoryStream
    {
        private class NotDisposableEmptyMemoryStream : MemoryStream
        {
            public NotDisposableEmptyMemoryStream() : base(EmptyArray.Get<byte>(), false)
            {
            }

            protected override void Dispose(bool disposing)
            {
            }
        }

        private static readonly MemoryStream s_instance = new NotDisposableEmptyMemoryStream();

        public static MemoryStream Get()
        {
            return s_instance;
        }
    }
}
