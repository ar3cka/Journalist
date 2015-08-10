using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.Extensions;

namespace Journalist.EventStore.Streams
{
    public class EventStreamReader : EventStreamInteractionEntity, IEventStreamReader
    {
        private readonly IEventMutationPipeline m_mutationPipeline;
        private readonly IEventStreamCursor m_streamCursor;
        private List<JournaledEvent> m_readedEvents;

        public EventStreamReader(
            string streamName,
            IEventStoreConnectionState connectionState,
            IEventStreamCursor streamCursor,
            IEventMutationPipeline mutationPipeline) : base(streamName, connectionState)
        {
            Require.NotNull(streamCursor, "streamCursor");
            Require.NotNull(mutationPipeline, "mutationPipeline");

            m_streamCursor = streamCursor;
            m_mutationPipeline = mutationPipeline;
        }

        public async Task ReadEventsAsync()
        {
            ConnectionState.EnsureConnectionIsActive();

            if (m_streamCursor.EndOfStream)
            {
                throw new InvalidOperationException("Stream \"{0}\" is empty.".FormatString(StreamName));
            }

            await m_streamCursor.FetchSlice();

            m_readedEvents = new List<JournaledEvent>(m_streamCursor.Slice.Count);
            foreach (var journaledEvent in m_streamCursor.Slice)
            {
                m_readedEvents.Add(m_mutationPipeline.Mutate(journaledEvent));
            }
        }

        public IReadOnlyList<JournaledEvent> Events
        {
            get
            {
                if (m_readedEvents != null)
                {
                    return m_readedEvents;
                }

                return EmptyArray.Get<JournaledEvent>();
            }
        }

        public override EventStreamPosition StreamPosition
        {
            get { return m_streamCursor.StreamPosition; }
        }

        public bool HasEvents
        {
            get
            {
                return !m_streamCursor.EndOfStream;
            }
        }
    }
}
