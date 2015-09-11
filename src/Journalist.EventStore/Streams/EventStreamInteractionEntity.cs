using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
    public abstract class EventStreamInteractionEntity : IEventStreamInteractionEntity
    {
        private readonly string m_streamName;
        private readonly IEventStoreConnectionState m_connectionState;

        protected EventStreamInteractionEntity(string streamName, IEventStoreConnectionState connectionState)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(connectionState, "connectionState");

            m_streamName = streamName;
            m_connectionState = connectionState;
        }

        public StreamVersion StreamVersion
        {
            get { return StreamHeader.Version; }
        }

        public abstract EventStreamHeader StreamHeader
        {
            get;
        }

        public string StreamName
        {
            get { return m_streamName; }
        }

        public bool IsClosed
        {
            get { return !m_connectionState.IsActive; }
        }

        protected IEventStoreConnectionState ConnectionState
        {
            get { return m_connectionState; }
        }
    }
}
