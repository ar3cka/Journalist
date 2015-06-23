using Journalist.WindowsAzure.Storage.Blobs;

namespace Journalist.EventStore.Streams
{
    public class EventStreamConsumingSessionFactory : IEventStreamConsumingSessionFactory
    {
        private readonly ICloudBlobContainer m_sessionsBlob;

        public EventStreamConsumingSessionFactory(ICloudBlobContainer sessionsBlob)
        {
            Require.NotNull(sessionsBlob, "sessionsBlob");

            m_sessionsBlob = sessionsBlob;
        }

        public IEventStreamConsumingSession CreateSession(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

           return new EventStreamConsumingSession(streamName, m_sessionsBlob);
        }
    }
}
