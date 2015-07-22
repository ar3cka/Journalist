using System;
using Journalist.EventStore.Streams;

namespace Journalist.EventStore.Connection
{
    public interface IEventStoreConnectionState
    {
        event EventHandler<EventStoreConnectivityStateEventArgs> ConnectionCreated;

        event EventHandler<EventStoreConnectivityStateEventArgs> ConnectionClosing;

        event EventHandler<EventStoreConnectivityStateEventArgs> ConnectionClosed;

        void EnsureConnectionIsActive();

        void ChangeToCreated();

        void ChangeToClosing();

        void ChangeToClosed();

        bool IsActive { get; }

        bool IsClosed { get; }
    }

}
