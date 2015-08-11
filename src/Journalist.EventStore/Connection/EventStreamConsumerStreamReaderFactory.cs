using System.Threading.Tasks;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public class EventStreamConsumerStreamReaderFactory : IEventStreamConsumerStreamReaderFactory
    {
        private readonly IEventStoreConnection m_connection;
        private readonly string m_streamName;
        private readonly bool m_startReadingFromTheEnd;
        private readonly StreamVersion m_readerStreamVersion;
        private readonly StreamVersion m_streamVersion;

        public EventStreamConsumerStreamReaderFactory(
            IEventStoreConnection connection,
            string streamName,
            bool startReadingFromTheEnd,
            StreamVersion readerStreamVersion,
            StreamVersion streamVersion)
        {
            Require.NotNull(connection, "connection");
            Require.NotEmpty(streamName, "streamName");

            m_connection = connection;
            m_streamName = streamName;
            m_startReadingFromTheEnd = startReadingFromTheEnd;
            m_readerStreamVersion = readerStreamVersion;
            m_streamVersion = streamVersion;
        }

        public Task<IEventStreamReader> CreateAsync()
        {
            var readerVersion = m_readerStreamVersion;
            if (readerVersion == StreamVersion.Unknown && m_startReadingFromTheEnd)
            {
                readerVersion = m_streamVersion;
            }

            return m_connection.CreateStreamReaderAsync(m_streamName, readerVersion.Increment());
        }
    }
}
