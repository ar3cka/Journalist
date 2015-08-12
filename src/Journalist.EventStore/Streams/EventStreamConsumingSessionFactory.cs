using System;
using Journalist.EventStore.Journal;
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


        public IEventStreamConsumingSession CreateSession(EventStreamReaderId consumerId, string streamName)
        {
            Require.NotNull(consumerId, "consumerId");
            Require.NotEmpty(streamName, "streamName");

            return new EventStreamConsumingSession(
               streamName,
               consumerId,
               TimeSpan.FromMinutes(Constants.Settings.DEFAULT_SESSION_LOCK_TIMEOUT_MINUTES),
               m_sessionsBlob);
        }
    }
}
