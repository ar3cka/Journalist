using System.Threading.Tasks;
using Journalist.EventSourced.Entities;
using Journalist.EventStore;
using Journalist.Options;

namespace Journalist.EventSourced.Application.Infrastructure.Storage
{
    public class EventStoreAggregateStateStorage<TState> : IAggregateStateStorage<TState>
        where TState : IAggregateState, new ()
    {
        private readonly IEventStoreConnection m_connection;

        public EventStoreAggregateStateStorage(IEventStoreConnection connection)
        {
            Require.NotNull(connection, "connection");

            m_connection = connection;
        }

        public async Task<Option<TState>> RestoreStateAsync(IIdentity identity)
        {
            Require.NotNull(identity, "identity");

            var state = new TState();
            var reader = await m_connection.CreateStreamReaderAsync("Q");
            while (reader.HasEvents)
            {
                await reader.ReadEventsAsync();

                state.Restore();
            }

            throw new System.NotImplementedException();
        }

        public Task PersistAsync(IIdentity identity, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}
