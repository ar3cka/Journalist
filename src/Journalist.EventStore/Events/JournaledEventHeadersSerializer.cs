using System;
using System.Collections.Generic;
using System.IO;
using Journalist.Collections;
using Journalist.IO;

namespace Journalist.EventStore.Events
{
    public static class JournaledEventHeadersSerializer
    {
        private static readonly string[] s_separators = ": ".YieldArray();

        public static MemoryStream Serialize(Dictionary<string, string> headers)
        {
            Require.NotNull(headers, "headers");

            if (headers.Count == 0)
            {
                return EmptyMemoryStream.Get();
            }

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                foreach (var eventHeader in headers)
                {
                    writer.WriteLine("{0}: {1}", eventHeader.Key, eventHeader.Value);
                }

                writer.Flush();

                return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Length, false);
            }
        }

        public static Dictionary<string, string> Deserialize(TextReader reader)
        {
            Require.NotNull(reader, "reader");

            var result = new Dictionary<string, string>();
            string pair;
            while ((pair = reader.ReadLine()) != null)
            {
                var keyValue = pair.Split(s_separators, StringSplitOptions.RemoveEmptyEntries);
                result.Add(keyValue[0], keyValue[1]);
            }

            return result;
        }

        public static Dictionary<string, string> Deserialize(string headers)
        {
            Require.NotEmpty(headers, "headers");

            using (var reader = new StringReader(headers))
            {
                return Deserialize(reader);
            }
        }

        public static Dictionary<string, string> Deserialize(Stream headers)
        {
            Require.NotNull(headers, "headers");

            using (var reader = new StreamReader(headers))
            {
                return Deserialize(reader);
            }
        }
    }
}
