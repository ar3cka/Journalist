namespace Journalist.EventStore.Streams
{
    public class EventStreamConnectivityState : IEventStreamConnectivityState
    {
        private bool m_isActive;
        private bool m_isClosing;
        private bool m_isClosed;

        public EventStreamConnectivityState()
        {
            m_isActive = true;
        }

        public void EnsureConnectionIsActive()
        {
            Ensure.True<EventStreamConnectionWasClosedException>(m_isActive);
        }

        public void ChangeToClosing()
        {
            Ensure.True(m_isActive, "Object is not in active state.");

            m_isActive = false;
            m_isClosing = true;
        }

        public void ChangeToClosed()
        {
            Ensure.True(m_isClosing, "Object is not in closing state.");

            m_isClosing = false;
            m_isClosed = true;
        }

        public bool IsActive
        {
            get { return m_isActive; }
        }

        public bool IsClosing
        {
            get { return m_isClosing; }
        }

        public bool IsClosed
        {
            get { return m_isClosed; }
        }
    }
}
