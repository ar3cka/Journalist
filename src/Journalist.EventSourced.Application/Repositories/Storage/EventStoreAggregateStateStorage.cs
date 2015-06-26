using System.Threading.Tasks;
using Journalist.EventSourced.Application.Serialization;
using Journalist.EventSourced.Entities;
using Journalist.EventStore;
using Journalist.EventStore.Journal;
using Journalist.Extensions;
using Journalist.Options;

namespace Journalist.EventSourced.Application.Repositories.Storage
{
    public class EventStoreAggregateStateStorage<TState> : IAggregateStateStorage<TState>
        where TState : IAggregateState, new ()
    {
        private readonly IEventStoreConnection m_connection;
        private readonly IEventSerializer m_serializer;

        public EventStoreAggregateStateStorage(IEventStoreConnection connection, IEventSerializer serializer)
        {
            Require.NotNull(connection, "connection");
            Require.NotNull(serializer, "serializer");

            m_connection = connection;
            m_serializer = serializer;
        }

        public async Task<Option<TState>> RestoreStateAsync(IIdentity identity)
        {
            Require.NotNull(identity, "identity");

            var streamName = StreamName.GetForIdentity(identity);

            var state = new TState();
            var reader = await m_connection.CreateStreamReaderAsync(streamName);
            if (reader.HasEvents)
            {
                while (reader.HasEvents)
                {
                    await reader.ReadEventsAsync();

                    var events = reader.Events.SelectToArray(journaledEvent => m_serializer.Deserialize(journaledEvent));
                    state.Restore(events);
                }

                state.StateWasRestored((int)reader.CurrentStreamVersion);

                return state.MayBe();
            }

            return Option.None();
        }

        public async Task PersistAsync(IIdentity identity, TState state)
        {
            Require.NotNull(identity, "identity");
            Require.NotNull(state, "state");

            var changes = state.Changes;
            if (changes.Count == 0)
            {
                return;
            }

            var streamName = StreamName.GetForIdentity(identity);
            var writer = await m_connection.CreateStreamWriterAsync(streamName);
            if ((int)writer.StreamVersion != state.OriginalStateVersion)
            {
                throw new AggregateWasConcurrentlyUpdatedException("Event stream \"{0}\" was concurrently updated.".FormatString(streamName));
            }

            try
            {
                var journaledEvents = state.Changes.SelectToArray(change => m_serializer.Serialize(change));
                await writer.AppendEventsAsync(journaledEvents);
                state.StateWasPersisted((int)writer.StreamVersion);
            }
            catch (EventStreamConcurrencyException exception)
            {
                throw new AggregateWasConcurrentlyUpdatedException("Event stream \"{0}\" was concurrently updated.".FormatString(streamName), exception);
            }
        }
    }
}
