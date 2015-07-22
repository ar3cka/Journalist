namespace Journalist.EventStore.Streams
{
    public abstract class EventStreamInteractionEntity : IEventStreamInteractionEntity
    {
        private readonly string m_streamName;
        private readonly IEventStreamConnectivityState m_connectivityState;

        protected EventStreamInteractionEntity(string streamName, IEventStreamConnectivityState connectivityState)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(connectivityState, "connectivityState");

            m_streamName = streamName;
            m_connectivityState = connectivityState;
        }

        public abstract StreamVersion StreamVersion
        {
            get;
        }

        public string StreamName
        {
            get { return m_streamName; }
        }

        public bool IsClosed
        {
            get { return !m_connectivityState.IsActive; }
        }

        protected IEventStreamConnectivityState ConnectivityState
        {
            get { return m_connectivityState; }
        }
    }
}
