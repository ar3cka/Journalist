namespace Journalist.EventStore.Notifications.Types
{
    public class EventStreamUpdated
    {
        public EventStreamUpdated(string streamName, StreamVersion fromVersion, StreamVersion toVersion)
        {
            Require.NotEmpty(streamName, "streamName");

            StreamName = streamName;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public string StreamName { get; private set; }

        public StreamVersion FromVersion { get; private set; }

        public StreamVersion ToVersion { get; private set; }
    }
}