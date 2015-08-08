using System;

namespace Journalist.EventStore.Connection
{
    public sealed class EventStoreConnectivityStateEventArgs : EventArgs
    {
        public EventStoreConnectivityStateEventArgs(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            Connection = connection;
        }

        public IEventStoreConnection Connection
        {
            get; private set;
        }
    }
}
