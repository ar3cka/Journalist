using System;
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

        public IEventStreamConsumingSession CreateSession(string consumerName, string streamName)
        {
            Require.NotEmpty(consumerName, "consumerName");
            Require.NotEmpty(streamName, "streamName");

            return new EventStreamConsumingSession(
               streamName,
               consumerName,
               TimeSpan.FromMinutes(Constants.Settings.DEFAULT_SESSION_LOCK_TIMEOUT_MINUTES),
               m_sessionsBlob);
        }
    }
}
