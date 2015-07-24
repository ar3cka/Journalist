using System;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public class EventStoreConnectionState : IEventStoreConnectionState
    {
        public event EventHandler<EventStoreConnectivityStateEventArgs> ConnectionCreated;
        public event EventHandler<EventStoreConnectivityStateEventArgs> ConnectionClosing;
        public event EventHandler<EventStoreConnectivityStateEventArgs> ConnectionClosed;

        private bool m_isInitial = true;
        private bool m_isActive;
        private bool m_isClosing;
        private bool m_isClosed;
        private IEventStoreConnection m_connection;

        public void EnsureConnectionIsActive()
        {
            Ensure.True<EventStreamConnectionWasClosedException>(m_isActive);
        }

        public void ChangeToCreated(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            Ensure.True(m_isInitial, "Object is not in initial state.");

            m_isInitial = false;
            m_isActive = true;

            m_connection = connection;

            OnConnectionCreated();
        }

        public void ChangeToClosing()
        {
            Ensure.True(m_isActive, "Object is not in active state.");

            m_isActive = false;
            m_isClosing = true;

            OnConnectionClosing();
        }

        public void ChangeToClosed()
        {
            Ensure.True(m_isClosing, "Object is not in closing state.");

            m_isClosing = false;
            m_isClosed = true;

            OnConnectionClosed();
        }

        private void OnConnectionCreated()
        {
            var handler = ConnectionCreated;
            if (handler != null)
            {
                handler(this, new EventStoreConnectivityStateEventArgs(m_connection));
            }
        }

        private void OnConnectionClosing()
        {
            var handler = ConnectionClosing;
            if (handler != null)
            {
                handler(this, new EventStoreConnectivityStateEventArgs(m_connection));
            }
        }

        private void OnConnectionClosed()
        {
            var handler = ConnectionClosed;
            if (handler != null)
            {
                handler(this, new EventStoreConnectivityStateEventArgs(m_connection));
            }
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
